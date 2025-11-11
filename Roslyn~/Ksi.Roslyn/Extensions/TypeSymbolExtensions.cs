using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn.Extensions;

public static class TypeSymbolExtensions
{
    public static bool TryGetGenericArg(this INamedTypeSymbol self, out INamedTypeSymbol? genericType)
    {
        genericType = null;

        if (!self.IsGenericType)
            return false;

        if (self.TypeParameters.Length != 1)
            return false;

        if (self.TypeArguments[0] is not INamedTypeSymbol gta)
            return false;

        genericType = gta;
        return true;
    }

    public static bool IsDynSizedOrWrapsDynSized(this ITypeSymbol self)
    {
        return self.Is(SymbolNames.DynSized) || self.WrapsDynSized();
    }

    public static bool WrapsDynSized(this ITypeSymbol self)
    {
        if (!self.IsRefLikeType || self is not INamedTypeSymbol { IsGenericType: true } nt)
            return false;

        if (nt.TypeArguments.Length != 1 || nt.TypeArguments.First() is not INamedTypeSymbol gt)
            return false;

        return nt.IsWrappedRef() && gt.IsDynSizedOrWrapsDynSized();
    }

    public static bool IsWellKnownGenericType(this INamedTypeSymbol self)
    {
        return self is { IsGenericType: true, TypeArguments.Length: 1 } &&
               (self.IsWrappedRef() || self.IsRefList() || self.IsExclusiveAccess());
    }

    public static bool IsSpanOrReadonlySpan(this ITypeSymbol self, out bool isMut)
    {
        isMut = false;

        if (!self.IsRefLikeType || self is not INamedTypeSymbol nt)
            return false;

        if (nt.IsSpan())
        {
            isMut = true;
            return true;
        }

        return nt.IsReadOnlySpan();
    }

    public static bool IsSpanOrReadonlySpan(this ITypeSymbol self) => self.IsSpanOrReadonlySpan(out _);

    public static bool IsSpan(this INamedTypeSymbol self) => self.IsRefLikeGeneric("System", "Span");
    private static bool IsReadOnlySpan(this INamedTypeSymbol self) => self.IsRefLikeGeneric("System", "ReadOnlySpan");

    public static bool IsAccessScope(this ITypeSymbol self) => self.IsAccessScope(out _);

    public static bool IsAccessScope(this ITypeSymbol self, out bool isMut)
    {
        isMut = false;

        if (!self.IsRefLikeType || self is not INamedTypeSymbol nt)
            return false;

        if (nt.IsMutableAccessScope())
        {
            isMut = true;
            return true;
        }

        return nt.IsReadOnlyAccessScope();
    }

    public static bool IsMutableAccessScope(this INamedTypeSymbol self) =>
        self.IsRefLikeGeneric("Ksi", "MutableAccessScope");

    private static bool IsReadOnlyAccessScope(this INamedTypeSymbol self) =>
        self.IsRefLikeGeneric("Ksi", "ReadOnlyAccessScope");

    private static bool IsRefLikeGeneric(this INamedTypeSymbol self, string ns, string name)
    {
        return self is { IsRefLikeType: true, IsGenericType: true, TypeArguments.Length: 1 }
               && self.ContainingNamespace.Name == ns && self.Name == name;
    }

    public static bool IsExclusiveAccess(this INamedTypeSymbol self) => self.IsGenericClass("Ksi", "ExclusiveAccess");

    private static bool IsGenericClass(this INamedTypeSymbol self, string ns, string name)
    {
        return self is { IsGenericType: true, TypeArguments.Length: 1, TypeKind: TypeKind.Class }
               && self.ContainingNamespace.Name == ns && self.Name == name;
    }

    public static bool IsWrappedRef(this ITypeSymbol self, out bool isMut)
    {
        return self.IsSpanOrReadonlySpan(out isMut) || self.IsAccessScope(out isMut);
    }

    public static bool IsWrappedRef(this ITypeSymbol self) => self.IsWrappedRef(out _);

    public static bool IsExplicitCopy(this ITypeSymbol self)
    {
        return self.IsStructOrTypeParameter() && self.Is(SymbolNames.ExplicitCopy);
    }

    public static bool IsDynSized(this ITypeSymbol self)
    {
        return self.IsStructOrTypeParameter() && self.Is(SymbolNames.DynSized);
    }

    public static bool IsTempAlloc(this ITypeSymbol self)
    {
        return self.IsStructOrTypeParameter() && self.Is(SymbolNames.TempAlloc);
    }

    public static bool IsDealloc(this ITypeSymbol self)
    {
        return self.IsStructOrTypeParameter() && self.Is(SymbolNames.Dealloc);
    }

    public static bool IsDeallocOrRefListOverDealloc(this ITypeSymbol self)
    {
        if (self.IsDealloc())
            return true;

        if (!self.IsRefList())
            return false;

        return self is INamedTypeSymbol nt && nt.TypeArguments.First().IsDealloc();
    }

    public static bool IsJaggedRefList(this INamedTypeSymbol self)
    {
        if (!self.IsRefList())
            return false;

        return self.TypeArguments.First() is INamedTypeSymbol gt && gt.IsRefList();
    }

    public static bool IsRefList(this INamedTypeSymbol self)
        => self.IsSingleArgGenericStruct() && self.Is(SymbolNames.RefList);

    public static bool IsRefListOfComponents(this INamedTypeSymbol self)
        => self.IsRefList() && self.TypeArguments[0].IsKsiComponent();

    public static bool IsRefListOfEntities(this INamedTypeSymbol self)
        => self.IsRefList() && self.TypeArguments[0].IsKsiEntity();

    public static bool IsRefListOfKsiHashTableSlot(this INamedTypeSymbol self)
        => self.IsRefList() && self.TypeArguments[0].IsKsiHashTableSlot();

    private static bool IsSingleArgGenericStruct(this INamedTypeSymbol self)
        => self is { TypeKind: TypeKind.Struct, IsGenericType: true, TypeArguments.Length: 1 };

    public static bool IsRefList(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.RefList);

    public static bool IsKsiComponent(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.KsiComponent);
    public static bool IsKsiEntity(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.KsiEntity);
    public static bool IsKsiArchetype(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.KsiArchetype);
    public static bool IsKsiDomain(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.KsiDomain);
    public static bool IsKsiHashTable(this ITypeSymbol self) => self.IsStruct() && self.Is(SymbolNames.KsiHashTable);

    public static bool IsKsiHashTableSlot(this ITypeSymbol self) =>
        self.IsStruct() && self.Is(SymbolNames.KsiHashTableSlot);

    public static bool IsKsiHandle(this ITypeSymbol self)
    {
        if (self is not INamedTypeSymbol { IsGenericType: false, Name: "KsiHandle" } t)
            return false;

        return t.ContainingType is { IsGenericType: false } ct && ct.IsKsiDomain();
    }

    public static bool IsKsiHastTableSlotState(this ITypeSymbol self)
    {
        return self is INamedTypeSymbol
        {
            TypeKind: TypeKind.Enum,
            ContainingNamespace.Name: SymbolNames.Ksi,
            Name: SymbolNames.KsiHashTableSlotState
        };
    }

    private static bool Is(this ITypeSymbol self, string attributeName)
    {
        return self.GetAttributes().Any(attribute => attribute.Is(attributeName));
    }

    public static bool IsStruct(this ITypeSymbol self) => self.TypeKind == TypeKind.Struct;
    public static bool IsValueType(this ITypeSymbol self) => self.TypeKind is TypeKind.Struct or TypeKind.Enum;


    public static bool IsStructOrTypeParameter(this ITypeSymbol self)
    {
        return self.TypeKind switch
        {
            TypeKind.Struct => true,
            TypeKind.TypeParameter => true,
            _ => false
        };
    }

    private static readonly SymbolDisplayFormat FullNameFormat = new SymbolDisplayFormat(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    public static string FullTypeName(this INamedTypeSymbol self) => self.ToDisplayString(FullNameFormat);

    public static Accessibility InAssemblyAccessibility(this ITypeSymbol self)
    {
        var accessibility = self.DeclaredAccessibility;

        while (true)
        {
            if (self.ContainingType == null)
                break;

            self = self.ContainingType;
            if (self.DeclaredAccessibility < accessibility)
                accessibility = self.DeclaredAccessibility;
        }

        return accessibility == Accessibility.ProtectedOrInternal ? Accessibility.Internal : accessibility;
    }

    public static bool IsTopLevel(this ITypeSymbol self) => self.ContainingType == null;

    public static bool IsPartial(this ITypeSymbol self, CancellationToken ct)
    {
        var syntax = self.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(ct);
        return syntax is TypeDeclarationSyntax tds && tds.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    public static bool AreUnique(this ImmutableArray<ITypeSymbol> self)
    {
        var types = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        return self.All(t => types.Add(t));
    }

    public static bool IsGloballyVisible(this ITypeSymbol self)
    {
        return self.SpecialType switch
        {
            SpecialType.System_Object => true,
            SpecialType.System_Enum => true,
            SpecialType.System_Void => true,
            SpecialType.System_Boolean => true,
            SpecialType.System_Char => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_String => true,
            _ => false
        };
    }

    public static bool IsConcreteType(this ITypeSymbol t)
    {
        if (t is not INamedTypeSymbol nt)
            return false;

        return !nt.IsGenericType || nt.TypeArguments.All(IsConcreteType);
    }
}
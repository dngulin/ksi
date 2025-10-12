# RefPathAttribute

A hint attribute for the reference path analyzer.
If the method returns a specific reference path, specify it with positional parameters,
otherwise omit them (that means the method can return any inner reference).

Indexers should be indicated with the "[n]" segments.
The "!" should be placed after the last `[SynSized]` segment.

Examples:
- `[RefList]` corresponds to the reference path `MethodName()`, meaning it can reference any inner data
- `[RefList("self", "!")]` corresponds to the reference path `self!`, meaning it doesn't contribute to the parent reference path
- `[RefList("self", "Field", "!", "[n]")]` corresponds to the reference path `self.Field![n]`

```csharp
public sealed class RefPathAttribute : Attribute
```

Constructors
- [RefPathAttribute\(\)](#refpathattribute) — non-explicit `[RefPath]` attribute constructor
- [RefPathAttribute\(params string\[\]\)](#refpathattributeparams-string) — explicit `[RefPath]` attribute constructor

Properties
- [Segments](#segments) — list of segments indicating the `RefPath` created by the marked extension method


## Constructors


### RefPathAttribute\(\)

Non-explicit `[RefPath]` attribute constructor.
Will be embedded into the calling expression `RefPath` as the method name suffixed with "()".

```csharp
public RefPathAttribute()
```


### RefPathAttribute\(params string\[\]\)

Explicit `[RefPath]` attribute constructor.
Will be embedded into the calling expression `RefPath` as a sequence of segments.

```csharp
public RefPathAttribute(params string[] segments)
```

Parameters
- `segments` — array of segments produced by return expression.
The `[DynSized]` separator "!" should be passed as a separated segment.


## Properties


### Segments

List of segments indicating the `RefPath` created by the marked extension method.
Is empty array in case of the non-explicit `RefPath`.
Can contain the `[DynSized]` separator "!".

```csharp
public string[] Segments
```

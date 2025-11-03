using System;

namespace Ksi
{
    /// <summary>
    /// <para>
    /// An attribute to mark a hashtable-based collection.
    /// It can be either <c>HashSet</c> or <c>HashMap</c> with API provided by Roslyn code generator.
    /// The generated implementation is based on the open addressing hash table with linear single-step probing
    /// and lazy deletion.
    /// </para>
    /// <para>
    /// Marked type should be a structure that defines:
    /// <list type="bullet">
    /// <item><description>
    /// <c>internal TRefList&lt;TSlot&gt; HashTable</c> — inner hash table,
    /// where <c>TSlot</c> should be marked with <see cref="KsiHashTableSlotAttribute"/>.
    /// Kind of the slot defines the collection kind (<c>HashSet</c> or <c>HashMap</c>)
    /// </description></item>
    /// <item><description><c>internal int Count</c> — count of occupied slots in the hash table</description></item>
    /// <item><description><c>internal int HashCode([in ]TKey key)</c> — computes hash code for the key defined in the <c>TSlot</c></description></item>
    /// <item><description><c>internal int AreEqual([in ]TKey l, [in ]TKey r)</c> — checks keys equality</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// You can receive <c>HashCode</c> and <c>AreEqual</c> parameters both by <c>in</c> and by value
    /// (except <see cref="ExplicitCopyAttribute">ExplicitCopy</see> keys, that should be passed only by <c>in</c>).
    /// </para>
    /// <para>
    /// Parameter reference kind used in the <c>HashCode</c> method is inherited by generated API.
    /// But for <see cref="ExplicitCopyAttribute">ExplicitCopy</see> keys in insertion methods
    /// it is always a <c>by value</c> parameter to enforce moving the key into the collection.
    /// </para>
    /// <para>
    /// It is recommended to define hash tables in a separate assembly to make their internal state unavailable.
    /// Use only the generated API to modify the collection state.
    /// </para>
    /// <para>
    /// <c>HashSet</c> API:
    /// <list type="bullet">
    /// <item><description><c>(in THashSet).Count()</c> — returns number of keys</description></item>
    /// <item><description><c>(in THashSet).Capacity()</c> — returns the hash table size</description></item>
    /// <item><description><c>(in THashSet).Contains([in ]TKey key)</c> — checks if the key exists in the hash table</description></item>
    /// <item><description><c>(ref THashSet).Add([in ]TKey key)</c> — adds a new key</description></item>
    /// <item><description><c>(ref THashSet).Remove([in ]TKey key)</c> — removes a key and returns a success flag</description></item>
    /// <item><description><c>(ref THashSet).Rebuild(int capacity)</c> — reallocates the hash set with a given hash table size</description></item>
    /// <item><description><c>(ref THashSet).Clear()</c> — clears the hash set</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <c>HashMap</c> API:
    /// <list type="bullet">
    /// <item><description><c>(in THashMap).Count()</c> — returns number of keys</description></item>
    /// <item><description><c>(in THashMap).Capacity()</c> — returns the hash table size</description></item>
    /// <item><description><c>(in THashMap).Contains(in TKey key, out int index)</c> — checks if the key exists in the hash table</description></item>
    /// <item><description><c>(in THashMap).RefReadonlyGet([in ]TKey key)</c> — returns a readonly <c>TValue</c> reference</description></item>
    /// <item><description><c>(in THashMap).RefReadonlyGetByIndex(int index)</c> — returns a readonly <c>TValue</c> reference</description></item>
    /// <item><description><c>(ref THashMap).RefGet([in ]TKey key)</c> — returns a mutable <c>TValue</c> reference</description></item>
    /// <item><description><c>(ref THashMap).RefGetByIndex(int index)</c> — returns a mutable <c>TValue</c> reference</description></item>
    /// <item><description><c>(ref THashMap).RefSet([in ]TKey key)</c> — finds an entry or creates a new one and returns a mutable <c>TValue</c> reference</description></item>
    /// <item><description><c>(ref THashMap).Remove([in ]TKey key)</c> — removes a key and returns a success flag</description></item>
    /// <item><description><c>(ref THashSet).Rebuild(int capacity)</c> — reallocates the hash set with a given hash table size</description></item>
    /// <item><description><c>(ref THashSet).Clear()</c> — clears the hash map</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class KsiHashTableAttribute : Attribute {}

    /// <summary>
    /// <para>
    /// An attribute to mark a hash table slot type.
    /// It is required to hint a code generator for <see cref="KsiHashTableAttribute">KsiHashTable</see> types.
    /// </para>
    /// <para>
    /// Marked type should be a structure that defines these fields:
    /// <list type="bullet">
    /// <item><description><c>internal KsiHashTableEntryState State</c> — hash table <see cref="KsiHashTableSlotState">slot state</see></description></item>
    /// <item><description><c>public TKey Key</c> — field to store the item key, should be a value type</description></item>
    /// <item><description>
    /// (optional) <c>public TValue Value</c> — field to store the item value, should be a value type.
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If you define the <c>Value</c> field, the structure will represent the <c>HashMap</c> slot.
    /// Otherwise, it will be a <c>HashSet</c> slot.
    /// </para>
    /// </summary>
    public class KsiHashTableSlotAttribute : Attribute {}
}
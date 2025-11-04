namespace Ksi.Roslyn
{
    public static class KsiHashTemplates
    {
        public const string HashSetApi =
            // language=cs
            """
            |accessibility| partial struct |THashSet|
            {
                /// <summary>
                /// Returns an empty hash set instance.
                /// </summary>
                public static |THashSet| Empty => default;
            
                /// <summary>
                /// Returns a new hash set instance with a capacity equal or greater of the given one.
                /// </summary>
                public static |THashSet| WithMinCapacity(int capacity)
                {
                    var set = Empty;
                    set.HashTable.AppendDefault(KsiPrimeUtil.EqualOrNextPrime(capacity));
                    return set;
                }
            }

            |accessibility| static class |THashSet|_KsiHashSetExtensions
            {
                /// <summary>
                /// Returns the number of keys stored in the hash set.
                /// </summary>
                /// <param name="self">The hash set to get key count</param>
                /// <returns>The number of keys stored in the hash set.</returns>
                public static int Count(in this |THashSet| self) => self.Count;
                
                /// <summary>
                /// Returns the hash set capacity.
                /// </summary>
                /// <param name="self">The hash set to get capacity</param>
                /// <returns>The number of slots allocated in the internal hash table.</returns>
                public static int Capacity(in this |THashSet| self) => self.HashTable.Count();
            
                /// <summary>
                /// Determines if the hash set contains a given key.
                /// </summary>
                /// <param name="self">The hash set to locate the key</param>
                /// <param name="key">The key to locate in the hash set</param>
                /// <returns><c>true</c> if the key exists in the hash set; otherwise, <c>false</c>.</returns>
                public static bool Contains(in this |THashSet| self, [in ]|TKey| key)
                {
                    return self.Count > 0 && self.SearchKey(key, out _);
                }
            
                /// <summary>
                /// Adds a new key to the hash set.
                /// </summary>
                /// <param name="self">The hash set to add the key</param>
                /// <param name="key">The key to add to the hash set</param>
                /// <returns><c>true</c> if the key was added to the hash set; otherwise, <c>false</c>.</returns>
                public static bool Add(ref this |THashSet| self, [in `insertion]|TKey| key)
                {
                    if (self.Count == self.Capacity())
                        self.Rebuild(self.Capacity() * 2);
            
                    var slotCount = self.Capacity();
                    var startIdx = self.GetStartIndex(key);
                    for (var i = 0; i < slotCount; i++)
                    {
                        ref var slot = ref self.HashTable.RefAt((startIdx + i) % slotCount);
                        switch (slot.State)
                        {
                            case KsiHashTableSlotState.Empty:
                                slot.Key = key[.Move()];
                                slot.State = KsiHashTableSlotState.Occupied;
                                self.Count++;
                                return true;
                            case KsiHashTableSlotState.Occupied when |THashSet|.Eq(key, slot.Key):
                                [key.Dealloc();
                                ]return false;
                        }
                    }
            
                    throw new System.Exception("Unreachable state on insertion");
                }
            
                /// <summary>
                /// Removes a key from the hash set.
                /// </summary>
                /// <param name="self">The hash set to remove the key</param>
                /// <param name="key">The key to remove from the hash set</param>
                /// <returns><c>true</c> if the key was removed from the hash set; otherwise, <c>false</c>.</returns>
                public static bool Remove(ref this |THashSet| self, [in ]|TKey| key)
                {
                    if (self.Count <= 0 || !self.SearchKey(key, out var idx))
                        return false;
            
                    ref var slot = ref self.HashTable.RefAt(idx);
                    slot[.Deallocated()`slot] = default;
                    self.Count--;
            
                    var nextIdx = (idx + 1) % self.Capacity();
                    if (self.HashTable.RefAt(nextIdx).State != KsiHashTableSlotState.Empty)
                        slot.State = KsiHashTableSlotState.Deleted;
                    else
                        self.TrimDeletedSlotsChain(idx);
            
                    return true;
                }
                
                /// <summary>
                /// Reallocates the hash set with a given minimal capacity.
                /// </summary>
                /// <param name="self">The hash set to rebuild</param>
                /// <param name="minCapacity">Minimal capacity</param>
                public static void Rebuild(ref this |THashSet| self, int minCapacity)
                {
                    var set = |THashSet|.WithMinCapacity(System.Math.Min(minCapacity, self.Count));
                
                    foreach (ref var slot in self.HashTable.RefIter())
                    {
                        if (slot.State == KsiHashTableSlotState.Occupied)
                            set.Add(slot.Key[.Move()]);
                    }
                
                    self[.Deallocated()`self] = set.Move();
                }
                
                /// <summary>
                /// Clears the hash set.
                /// </summary>
                /// <param name="self">The hash set to clear</param>
                public static void Clear(ref this |THashSet| self)
                {
                    self.HashTable.Clear();
                    self.HashTable.AppendDefault(self.HashTable.Capacity());
                    self.Count = 0;
                }
            
                private static bool SearchKey(this in |THashSet| self, [in ]|TKey| key, out int idx)
                {
                    idx = 0;
                    var slotCount = self.Capacity();
                    if (slotCount <= 0)
                        return false;
            
                    var startIdx = self.GetStartIndex(key);
                    for (var i = 0; i < slotCount; i++)
                    {
                        idx = (startIdx + i) % slotCount;
                        ref readonly var slot = ref self.HashTable.RefReadonlyAt(idx);
                        switch (slot.State)
                        {
                            case KsiHashTableSlotState.Empty:
                                return false;
                            case KsiHashTableSlotState.Occupied when |THashSet|.Eq(key, slot.Key):
                                return true;
                        }
                    }
            
                    return false;
                }
                
                private static int GetStartIndex(this in |THashSet| self, [in ]|TKey| key)
                {
                    return (int)((uint)|THashSet|.Hash(key) % (uint)self.Capacity());
                }
            
                private static void TrimDeletedSlotsChain([DynNoResize] ref this |THashSet| self, int startIdx)
                {
                    startIdx -= 1;
            
                    var slotCount = self.Capacity();
                    for (var i = 0; i < slotCount - 1; i++)
                    {
                        var idx = (slotCount + startIdx - i) % slotCount;
                        ref var slot = ref self.HashTable.RefAt(idx);
                        if (slot.State != KsiHashTableSlotState.Deleted)
                            return;
            
                        slot.State = KsiHashTableSlotState.Empty;
                    }
                }
            }
            """;

        public const string HashMapApi =
            // language=cs
            """
            |accessibility| partial struct |THashMap|
            {
                /// <summary>
                /// Returns an empty hash map instance.
                /// </summary>
                public static |THashMap| Empty => default;
            
                /// <summary>
                /// Returns a new hash map instance with a capacity equal or greater of the given one.
                /// </summary>
                public static |THashMap| WithMinCapacity(int capacity)
                {
                    var map = Empty;
                    map.HashTable.AppendDefault(KsiPrimeUtil.EqualOrNextPrime(capacity));
                    return map;
                }
            }

            |accessibility| static class |THashMap|_KsiHashMapExtensions
            {
                /// <summary>
                /// Returns the number of keys stored in the hash map.
                /// </summary>
                /// <param name="self">The hash map to get key count</param>
                /// <returns>The number of keys stored in the hash map.</returns>
                public static int Count(in this |THashMap| self) => self.Count;
                
                /// <summary>
                /// Returns the hash map capacity.
                /// </summary>
                /// <param name="self">The hash map to get capacity</param>
                /// <returns>The number of slots allocated in the internal hash table.</returns>
                public static int Capacity(in this |THashMap| self) => self.HashTable.Count();
            
                /// <summary>
                /// Determines if the hash map contains a given key.
                /// </summary>
                /// <param name="self">The hash map to locate the key</param>
                /// <param name="key">The key to locate in the hash map</param>
                /// <param name="index">Index of the slot containing the key</param>
                /// <returns><c>true</c> if the key exists in the hash map; otherwise, <c>false</c>.</returns>
                public static bool Contains(in this |THashMap| self, [in ]|TKey| key, out int index)
                {
                    index = 0;
                    var slotCount = self.Capacity();
                    if (slotCount <= 0)
                        return false;
                    
                    var startIdx = self.GetStartIndex(key);
                    for (var i = 0; i < slotCount; i++)
                    {
                        index = (startIdx + i) % slotCount;
                        ref readonly var slot = ref self.HashTable.RefReadonlyAt(index);
                        switch (slot.State)
                        {
                            case KsiHashTableSlotState.Empty:
                                return false;
                            case KsiHashTableSlotState.Occupied when |THashMap|.Eq(key, slot.Key):
                                return true;
                        }
                    }
                    
                    return false;
                }
                
                /// <summary>
                /// Gets a readonly value reference stored in the hash map.
                /// </summary>
                /// <param name="self">The hash map to get the value</param>
                /// <param name="key">The key to locate in the hash map</param>
                /// <returns>A readonly reference to the value associated with the key.</returns>
                /// <exception cref="System.Collections.Generic.KeyNotFoundException">
                /// If the key is not located in the hash map.
                /// </exception>
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref readonly |TValue| RefReadonlyGet(in this |THashMap| self, [in ]|TKey| key)
                {
                    if (self.Contains(key, out var index))
                        return ref self.HashTable.RefReadonlyAt(index).Value;
                    
                    throw new System.Collections.Generic.KeyNotFoundException();
                }
                
                /// <summary>
                /// Gets a readonly value reference stored in the hash map.
                /// </summary>
                /// <param name="self">The hash map to get the value</param>
                /// <param name="index">The index in the internal hash table to get the key</param>
                /// <returns>A readonly reference to the value stored in the hash map.</returns>
                /// <exception cref="System.IndexOutOfRangeException">
                /// If the index is out of bounds of the internal hash table.
                /// </exception>
                /// <exception cref="System.Collections.Generic.KeyNotFoundException">
                /// If the index doesn't point to an occupied slot.
                /// </exception>
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref readonly |TValue| RefReadonlyGetByIndex(in this |THashMap| self, int index)
                {
                    ref readonly var slot = ref self.HashTable.RefReadonlyAt(index);
                    
                    if (slot.State != KsiHashTableSlotState.Occupied)
                        throw new System.Collections.Generic.KeyNotFoundException();
                    
                    return ref slot.Value;
                }
                
                /// <summary>
                /// Gets a mutable value reference stored in the hash map.
                /// </summary>
                /// <param name="self">The hash map to get the value</param>
                /// <param name="key">The key to locate in the hash map</param>
                /// <returns>A mutable reference to the value associated with the key.</returns>
                /// <exception cref="System.Collections.Generic.KeyNotFoundException">
                /// If the key is not located in the hash map.
                /// </exception>
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref |TValue| RefGet([DynNoResize] ref this |THashMap| self, [in ]|TKey| key)
                {
                    if (self.Contains(key, out var index))
                        return ref self.HashTable.RefAt(index).Value;
                    
                    throw new System.Collections.Generic.KeyNotFoundException();
                }
            
                /// <summary>
                /// Gets a mutable value reference stored in the hash map.
                /// </summary>
                /// <param name="self">The hash map to get the value</param>
                /// <param name="index">The index in the internal hash table to get the key</param>
                /// <returns>A mutable reference to the value stored in the hash map.</returns>
                /// <exception cref="System.IndexOutOfRangeException">
                /// If the index is out of bounds of the internal hash table.
                /// </exception>
                /// <exception cref="System.Collections.Generic.KeyNotFoundException">
                /// If the index doesn't point to an occupied slot.
                /// </exception>
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref |TValue| RefGetByIndex([DynNoResize] ref this |THashMap| self, int index)
                {
                    ref var slot = ref self.HashTable.RefAt(index);
                    
                    if (slot.State != KsiHashTableSlotState.Occupied)
                        throw new System.Collections.Generic.KeyNotFoundException();
                    
                    return ref slot.Value;
                }
            
                /// <summary>
                /// Optionally inserts a new key and returns a mutable reference to the associated value.
                /// </summary>
                /// <param name="self">The hash map to get the value</param>
                /// <param name="key">The key to locate or insert to the hash map</param>
                /// <returns>A mutable reference to the value associated with the key.</returns>
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref |TValue| RefSet(ref this |THashMap| self, [in `insertion]|TKey| key)
                {
                    if (self.Count == self.Capacity())
                        self.Rebuild(self.Capacity() * 2);
            
                    var slotCount = self.Capacity();
                    var startIdx = self.GetStartIndex(key);
                    for (var i = 0; i < slotCount; i++)
                    {
                        ref var slot = ref self.HashTable.RefAt((startIdx + i) % slotCount);
                        switch (slot.State)
                        {
                            case KsiHashTableSlotState.Empty:
                                slot.Key = key[.Move()`key];
                                slot.State = KsiHashTableSlotState.Occupied;
                                self.Count++;
                                return ref slot.Value;
                            case KsiHashTableSlotState.Occupied when |THashMap|.Eq(key, slot.Key):
                                [key.Dealloc();
                                ]return ref slot.Value;
                        }
                    }
            
                    throw new System.Exception("Unreachable state on insertion");
                }
            
                /// <summary>
                /// Removes a key from the hash map.
                /// </summary>
                /// <param name="self">The hash map to remove the key</param>
                /// <param name="key">The key to remove from the hash map</param>
                /// <returns><c>true</c> if the key was removed from the hash map; otherwise, <c>false</c>.</returns>
                public static bool Remove(ref this |THashMap| self, [in ]|TKey| key)
                {
                    if (self.Count <= 0 || !self.Contains(key, out var idx))
                        return false;
            
                    ref var slot = ref self.HashTable.RefAt(idx);
                    slot[.Deallocated()`slot] = default;
                    self.Count--;
            
                    var nextIdx = (idx + 1) % self.Capacity();
                    if (self.HashTable.RefAt(nextIdx).State != KsiHashTableSlotState.Empty)
                        slot.State = KsiHashTableSlotState.Deleted;
                    else
                        self.TrimDeletedSlotsChain(idx);
            
                    return true;
                }
                
                /// <summary>
                /// Reallocates the hash map with a given minimal capacity.
                /// </summary>
                /// <param name="self">The hash map to rebuild</param>
                /// <param name="minCapacity">Minimal capacity</param>
                public static void Rebuild(ref this |THashMap| self, int minCapacity)
                {
                    var map = |THashMap|.WithMinCapacity(System.Math.Min(minCapacity, self.Count));
                
                    foreach (ref var slot in self.HashTable.RefIter())
                    {
                        if (slot.State == KsiHashTableSlotState.Occupied)
                            map.RefSet(slot.Key[.Move()`key]) = slot.Value[.Move()`value];
                    }
                
                    self[.Deallocated()`self] = map.Move();
                }
                
                /// <summary>
                /// Clears the hash map.
                /// </summary>
                /// <param name="self">The hash map to clear</param>
                public static void Clear(ref this |THashMap| self)
                {
                    self.HashTable.Clear();
                    self.HashTable.AppendDefault(self.HashTable.Capacity());
                    self.Count = 0;
                }
                
                private static int GetStartIndex(this in |THashMap| self, [in ]|TKey| key)
                {
                    return (int)((uint)|THashMap|.Hash(key) % (uint)self.Capacity());
                }
            
                private static void TrimDeletedSlotsChain([DynNoResize] ref this |THashMap| self, int startIdx)
                {
                    startIdx -= 1;
            
                    var slotCount = self.Capacity();
                    for (var i = 0; i < slotCount - 1; i++)
                    {
                        var idx = (slotCount + startIdx - i) % slotCount;
                        ref var slot = ref self.HashTable.RefAt(idx);
                        if (slot.State != KsiHashTableSlotState.Deleted)
                            return;
            
                        slot.State = KsiHashTableSlotState.Empty;
                    }
                }
            }
            """;
    }
}
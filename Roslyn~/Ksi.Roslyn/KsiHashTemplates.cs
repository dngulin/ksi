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
                /// Returns an empty HashSet instance.
                /// </summary>
                public static |THashSet| Empty => default;
            
                /// <summary>
                /// Returns a HashSet instance with a capacity equal or greater of the given one.
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
                public static int Count(in this |THashSet| self) => self.Count;
                
                public static int Capacity(in this |THashSet| self) => self.HashTable.Count();
            
                public static bool Contains(in this |THashSet| self, [in ]|TKey| key)
                {
                    return self.Count > 0 && self.SearchKey(key, out _);
                }
            
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
                /// Returns an empty HashMap instance.
                /// </summary>
                public static |THashMap| Empty => default;
            
                /// <summary>
                /// Returns a HashMap instance with a capacity equal or greater of the given one.
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
                public static int Count(in this |THashMap| self) => self.Count;
                
                public static int Capacity(in this |THashMap| self) => self.HashTable.Count();
            
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
                
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref readonly |TValue| RefReadonlyGet(in this |THashMap| self, [in ]|TKey| key)
                {
                    if (self.Contains(key, out var index))
                        return ref self.HashTable.RefReadonlyAt(index).Value;
                    
                    throw new System.Collections.Generic.KeyNotFoundException();
                }
                
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref readonly |TValue| RefReadonlyGetByIndex(in this |THashMap| self, int index)
                {
                    ref readonly var slot = ref self.HashTable.RefReadonlyAt(index);
                    
                    if (slot.State != KsiHashTableSlotState.Occupied)
                        throw new System.Collections.Generic.KeyNotFoundException();
                    
                    return ref slot.Value;
                }
                
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref |TValue| RefGet([DynNoResize] ref this |THashMap| self, [in ]|TKey| key)
                {
                    if (self.Contains(key, out var index))
                        return ref self.HashTable.RefAt(index).Value;
                    
                    throw new System.Collections.Generic.KeyNotFoundException();
                }
            
                [RefPath("self", "HashTable", |RefPathSuffix|)]
                public static ref |TValue| RefGetByIndex([DynNoResize] ref this |THashMap| self, int index)
                {
                    ref var slot = ref self.HashTable.RefAt(index);
                    
                    if (slot.State != KsiHashTableSlotState.Occupied)
                        throw new System.Collections.Generic.KeyNotFoundException();
                    
                    return ref slot.Value;
                }
            
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
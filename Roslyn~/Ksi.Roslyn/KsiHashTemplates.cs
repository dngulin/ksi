namespace Ksi.Roslyn
{
    public static class KsiHashTemplates
    {
        public const string HashSetApi =
            // language=cs
            """
            |accessibility| partial struct |THashTable|
            {
                /// <summary>
                /// Returns an empty HashSet instance.
                /// </summary>
                public static |THashTable| Empty => default;
            
                /// <summary>
                /// Returns a HashSet instance with a capacity equal or greater of the given one.
                /// </summary>
                public static |THashTable| WithMinCapacity(int capacity)
                {
                    var set = Empty;
                    set.HashTable.AppendDefault(KsiPrimeUtil.EqualOrNextPrime(capacity));
                    return set;
                }
            }

            |accessibility| static class |THashTable|_KsiHashSetExtensions
            {
                public static int Count(in this |THashTable| self) => self.Count;
                
                public static int Capacity(in this |THashTable| self) => self.HashTable.Count();
            
                public static bool Contains(in this |THashTable| self, in |TKey| key)
                {
                    return self.Count > 0 && self.SearchKey(key, out _);
                }
            
                public static void Rebuild(ref this |THashTable| self, int minCapacity)
                {
                    var set = |THashTable|.WithMinCapacity(System.Math.Min(minCapacity, self.Count));
            
                    foreach (ref var slot in self.HashTable.RefIter())
                    {
                        if (slot.State == KsiHashTableSlotState.Occupied)
                            set.Add(slot.Key.Move());
                    }
            
                    self.Deallocated() = set.Move();
                }
            
                private static int GetStartIndex(this in |THashTable| self, in |TKey| key)
                {
                    return (int)((uint)|THashTable|.Hash(key) % (uint)self.Capacity());
                }
            
                public static bool Add(ref this |THashTable| self, [in ]|TKey| key)
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
                            case KsiHashTableSlotState.Occupied when |THashTable|.Eq(key, slot.Key):
                                [key.Dealloc();
                                ]return false;
                        }
                    }
            
                    throw new System.Exception("Unreachable state on adding an item");
                }
            
                public static bool Remove(ref this |THashTable| self, in |TKey| key)
                {
                    if (self.Count <= 0 || !self.SearchKey(key, out var idx))
                        return false;
            
                    ref var slot = ref self.HashTable.RefAt(idx);
                    slot[.Deallocated()] = default;
                    self.Count--;
            
                    var nextIdx = (idx + 1) % self.Capacity();
                    if (self.HashTable.RefAt(nextIdx).State != KsiHashTableSlotState.Empty)
                        slot.State = KsiHashTableSlotState.Deleted;
                    else
                        self.TrimDeletedSlotsChain(idx);
            
                    return true;
                }
            
                private static bool SearchKey(this in |THashTable| self, in |TKey| key, out int idx)
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
                            case KsiHashTableSlotState.Occupied when |THashTable|.Eq(key, slot.Key):
                                return true;
                        }
                    }
            
                    return false;
                }
            
                private static void TrimDeletedSlotsChain([DynNoResize] ref this |THashTable| self, int startIdx)
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
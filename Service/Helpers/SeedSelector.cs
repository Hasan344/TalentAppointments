using System;
using System.Collections.Generic;
using System.Linq;

namespace ForQab.Service.Helpers
{
    public static class SeededSelector
    {
        // FNV-1a — platform/runtime müstəqil, tam deterministik.
        // Eyni seed + eyni id => həmişə eyni açar (hər .NET versiyasında).
        public static long OrderKey(int[] seed, int entityId)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;   // FNV offset basis
                const ulong prime = 1099511628211UL;  // FNV prime

                void Mix(int value)
                {
                    hash ^= (byte)(value & 0xFF); hash *= prime;
                    hash ^= (byte)((value >> 8) & 0xFF); hash *= prime;
                    hash ^= (byte)((value >> 16) & 0xFF); hash *= prime;
                    hash ^= (byte)((value >> 24) & 0xFF); hash *= prime;
                }

                for (int i = 0; i < seed.Length; i++) Mix(seed[i]);
                Mix(entityId);
                return unchecked((long)hash);
            }
        }

        // Ən az ThisYearAssignmentCount-dan başlayaraq, qrup daxilində seed-ə görə
        // deterministik sıralanmış TAM siyahı qaytarır.
        public static List<T> Order<T>(
            IEnumerable<T> candidates,
            int[] seed,
            Func<T, int> idSelector,
            Func<T, int> countSelector)
        {
            return candidates
                .GroupBy(countSelector)
                .OrderBy(g => g.Key)
                .SelectMany(g => g.OrderBy(x => OrderKey(seed, idSelector(x)))
                                  .ThenBy(idSelector))   // hash toqquşması olarsa belə tam deterministik
                .ToList();
        }

        // Form-dan gələn 4 rəqəmi yoxlayır.
        public static int[] Validate(int[]? seed)
        {
            if (seed == null || seed.Length != 3 || seed.All(s => s == 0))
                throw new ArgumentException("Random seed daxil edilməlidir (3 rəqəm).");
            return seed;
        }

        // Snapshot serializasiyası: "id:count,id:count,..."
        public static string SerializePool<T>(IEnumerable<T> pool, Func<T, int> id, Func<T, int> count)
            => string.Join(",", pool.Select(x => $"{id(x)}:{count(x)}"));

        public static List<(int Id, int Count)> ParsePool(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(p => p.Split(':'))
                      .Select(p => (int.Parse(p[0]), int.Parse(p[1])))
                      .ToList();
        }

        public static string SerializeIds(IEnumerable<int> ids) => string.Join(",", ids);

        public static List<int> ParseIds(string? csv)
            => string.IsNullOrWhiteSpace(csv)
               ? new()
               : csv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();


    }
}
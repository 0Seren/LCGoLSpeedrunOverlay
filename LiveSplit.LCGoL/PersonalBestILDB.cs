using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.LCGoL
{
	internal class PersonalBestIldb
    {
        public delegate void NewPersonalBestEventArgs(object sender, string level, TimeSpan time, TimeSpan oldTime);

        private class NamedDeepPointer : DeepPointer
        {
            public string Name { get; set; }

            public NamedDeepPointer(int @base, params int[] offsets)
                : base(@base, offsets)
            {
            }
        }

        private readonly Dictionary<NamedDeepPointer, uint> _db;

        private const int _prTableStart = 14164384;

        private const int _structSize = 240;

        private readonly string[] _levels = {
            "Temple Grounds", "Spider Tomb", "The Summoning", "Toxic Swamp", "Flooded Passage", "Temple of Light", "The Jaws of Death", "Forgotten Gate", "Twisting Bridge", "Belly of the Beast",
            "Xolotl's Stronghold", "The Mirror's Wake", "Fiery Depths", "Stronghold Passage",
        };

        public event NewPersonalBestEventArgs OnNewIlPersonalBest;

        public PersonalBestIldb()
        {
            _db = new Dictionary<NamedDeepPointer, uint>();
            for (int i = 0; i < _levels.Length; i++)
            {
                int num = _prTableStart + _structSize * i;
                var key = new NamedDeepPointer(num)
                {
                    Name = "Solo - " + _levels[i]
                };
                var key2 = new NamedDeepPointer(num + 8)
                {
                    Name = "Coop - " + _levels[i]
                };
                _db.Add(key, 0u);
                _db.Add(key2, 0u);
            }
        }

        public void Update(Process game)
        {
            foreach (var item in new List<NamedDeepPointer>(_db.Keys))
            {
                uint num = _db[item];
                var num2 = item.Deref<uint>(game);
                if (num2 != num)
                {
                    _db[item] = num2;
                    if (num2 < num && num2 != 0)
                    {
                        OnNewIlPersonalBest?.Invoke(this, item.Name, TimeSpan.FromMilliseconds(num2), TimeSpan.FromMilliseconds(num));
                    }
                }
            }
        }
    }
}

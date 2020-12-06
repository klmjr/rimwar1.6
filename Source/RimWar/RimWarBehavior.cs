using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWar
{
    public enum RimWarBehavior : byte
    {
        Expansionist,
        Cautious,
        Merchant,
        Aggressive,
        Warmonger,
        Random,
        Player,
        Vassal,
        Excluded,
        Undefined
    }
}

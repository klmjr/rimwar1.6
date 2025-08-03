# RimWar 1.6 - Developer TODO List 🔧

## 🚨 **CRITICAL FIXES (Priority 1)**

### **1. RimWarSettlementComp.GetGizmos() Null Reference**
```csharp
// Location: Source/RimWar/Planet/RimWarSettlementComp.cs
// Error: System.NullReferenceException at GetGizmos()
// Fix: Add null checks before yielding gizmos
```
**Action Required**: Find and fix null reference in GetGizmos() iterator

### **2. WorldPath Index Out of Range**
```csharp
// Location: Source/RimWar/Planet/WarObject_PathFollower.cs  
// Error: ArgumentOutOfRangeException in WorldPath.Peek()
// Fix: Add bounds checking before accessing path nodes
```
**Action Required**: Add safety checks in SetupMoveIntoNextTile() and TryEnterNextPathTile()

### **3. WorldComponent_PowerTracker Null References**
```csharp
// Location: Source/RimWar/Planet/WorldComponent_PowerTracker.cs
// Error: NullReferenceException in AttemptLaunchedWarbandAgainstTownOnMainThread()
// Fix: Add null parameter validation
```
**Action Required**: Add null checks for rwd, parentSettlement, rwsComp parameters

## 🔧 **FUNCTIONALITY FIXES (Priority 2)**

### **4. Combat Group Definitions Missing**
```
Error: "New Arrivals attempted to execute raid but has no defined combat groups"
```
**Action Required**: 
- Investigate faction def changes in 1.6
- Ensure combat groups are properly defined for all factions
- Check PawnGroupKindDef compatibility

### **5. SpaceSettlement Component Discovery**
```
Error: "no rwsc found for SpaceSettlement"
```
**Action Required**:
- Check if SpaceSettlement is a new 1.6 settlement type
- Update settlement component detection logic
- Add compatibility for new settlement types

## 📋 **CODE MIGRATION (Priority 3)**

### **6. TransportPod → Transporter Migration**
**Status**: 🔧 In Progress
**Location**: Various files in Source/RimWar/Planet/
**Action Required**: Complete the systematic replacement of TransportPod references

### **7. 1.6 API Compatibility**
**Status**: 📋 Planned
**Action Required**: 
- Review all RimWorld API calls for 1.6 changes
- Update deprecated method calls
- Test new 1.6 features integration

## 🧪 **TESTING CHECKLIST**

### **Runtime Stability**
- [ ] Fix all NullReferenceExceptions
- [ ] Fix all ArgumentOutOfRangeExceptions  
- [ ] Verify no infinite error loops
- [ ] Test faction AI pathfinding
- [ ] Test combat initiation

### **Gameplay Features**
- [ ] Faction relations work correctly
- [ ] Settlement generation functions
- [ ] Warband movement and combat
- [ ] Player interaction systems
- [ ] Save/load compatibility

### **UI/UX**
- [ ] All gizmos display properly
- [ ] Settlement inspection works
- [ ] World map interactions functional
- [ ] No UI freezing or errors

## 🎯 **IMMEDIATE NEXT STEPS**
1. **Fix GetGizmos() null reference** - Most frequent error
2. **Fix WorldPath bounds checking** - Prevents warband movement  
3. **Add PowerTracker null validation** - Stops combat errors
4. **Test basic faction interactions** - Verify core gameplay

## 📝 **CODE REVIEW NOTES**
- Many errors are safety issues (null checks, bounds checking)
- Core logic appears sound, just needs defensive programming
- 1.6 compatibility mostly structural, not algorithmic changes
- Most fixes are additive (safety checks) rather than rewrites

## 🔍 **DEBUGGING STRATEGY**
1. **Null Reference Errors**: Add comprehensive null checking
2. **Index Errors**: Add bounds validation everywhere
3. **Component Errors**: Update type checking for 1.6 objects
4. **Compatibility**: Test with minimal mod setup first

---
*Developer Notes: Focus on defensive programming - add null checks and bounds validation throughout. The core mod
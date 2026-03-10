# Bug Fix: FishActionNode NullReference Crash
**Date**: 2026-03-05
**File**: Assets\FishingFramework\Module\Fish\FishAction\BTTree\Action\FishActionNode.cs
## Problem Description
In the ChangeForceDynamic method, logic was accessing .Value on a nullable float (loat?) without checking if it was null.
`csharp
// Risky Code
SetForceTarget(actionData.InitForce.Value * ...);
`
If ctionData.InitForce is null (e.g., initialization timing issue), this throws System.InvalidOperationException.
## Solution
Use .GetValueOrDefault() to safely unwrap the nullable type.
`csharp
// Safe Code
SetForceTarget(actionData.InitForce.GetValueOrDefault() * ...);
`
## Best Practice
When working with loat? or int? in simulation logic, always provide a default value or check for null to prevent runtime crashes during gameplay.

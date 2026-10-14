BOAT SYSTEM - PHASE 1

Scripts:
- BoatMode.cs
- BoatInteractable.cs
- BoatPointingInteractor.cs
- BoatPaddle.cs
- BoatMovement.cs
- BoatModeLook.cs

SETUP

1. Keep the existing boat model hierarchy unchanged:
   thuyen
   - Boat.OarRing.L
     - Boat.Oar.L
   - Boat.OarRing.R
     - Boat.Oar.R
   - Boat.Seat
   - Boat.Cover.Raw.001
   - Boat.Rudder.Hinge
     - Boat.Rudder

2. On thuyen, add:
   - Rigidbody
       Is Kinematic = ON
       Use Gravity = OFF
   - BoatMode
   - BoatMovement
   - BoatInteractable (or put BoatInteractable on a child interaction collider)

3. Under thuyen create:
   BoatGameplay
   - SeatPoint
   - ExitPoint
   - LeftPaddle
     - GripPoint
   - RightPaddle
     - GripPoint
   - InteractionPoint

4. SeatPoint:
   Position it where the player's XR Origin should be placed when entering.

5. ExitPoint:
   Position it at the middle/exit location where the player should appear after leaving.

6. LeftPaddle / RightPaddle:
   These are gameplay objects, not the existing model objects.
   Add BoatPaddle to each.
   Assign:
   - Visual = existing Boat.Oar.L / Boat.Oar.R model
   - GripPoint = its child GripPoint
   - HandController = the corresponding simulation controller
   - LeftHand / RightHand = the existing VRSimulationLeftHand / VRSimulationRightHand
   Set Hand Side accordingly.

7. BoatMovement:
   Assign Left Paddle and Right Paddle.
   Set Water Height to the Y level at which the boat should remain.
   The boat is moved kinematically; it is not buoyancy physics.

8. BoatMode:
   Assign VRSimulationPlayer, XR Origin, SeatPoint, ExitPoint and BoatInteractable.
   F and Gamepad North enter/exit.

9. Pointing:
   Add BoatPointingInteractor to a suitable player/controller object.
   Assign the left/right controller transforms and the boat interactable.
   The current implementation uses the hand ray plus the corresponding gamepad trigger.

10. BoatModeLook:
    Optional. Assign BoatMode, XR Origin and the same look pitch pivot used by the simulator.
    It provides mouse look while BoatMode is active.

IMPORTANT
- Do not edit VRSimulationControls.cs.
- Do not add Rigidbody components to the individual model meshes.
- This is the first implementation pass. Rowing force, paddle return-to-rest pose,
  highlight feedback, and more precise VR interaction can be refined after the base
  system compiles and runs in Unity.

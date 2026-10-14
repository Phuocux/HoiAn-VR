using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Boat Paddle
///
/// StrokeInput chỉ phản ánh chuyển động của tay
/// TƯƠNG ĐỐI VỚI THUYỀN.
///
/// Vì vậy:
/// - Tay đứng yên + thuyền di chuyển = StrokeInput 0
/// - Tay đứng yên + thuyền xoay = StrokeInput 0
/// - Tay thực sự kéo/đẩy mái chèo = StrokeInput có giá trị
///
/// Mechanical behaviour:
/// 1. Grip prompt must be visible before pressing Grip.
/// 2. Press Grip once = grab.
/// 3. Press Grip again = release.
/// 4. While held, GripPoint follows the hand exactly.
/// 5. OarPivot is the fixed support point.
/// 6. Paddle rotates around OarPivot.up.
/// 7. Paddle can slide along its shaft through OarPivot.
/// 8. The paddle keeps its horizontal orientation.
/// </summary>
public class BoatPaddle : MonoBehaviour
{
    public enum HandSide
    {
        Left,
        Right
    }

    [Header("References")]
    [SerializeField] private Transform visual;
    [SerializeField] private Transform gripPoint;
    [SerializeField] private Transform handController;
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    [Header("Oar Pivot")]
    [SerializeField] private Transform oarPivot;

    [Header("Boat Mode")]
    [SerializeField] private BoatMode boatMode;

    [Header("Row Axis")]
    [SerializeField] private Vector3 localStrokeAxis = Vector3.down;

    [Header("Grab")]
    [SerializeField] private HandSide handSide;
    [SerializeField] private float grabDistance = 0.30f;
    [SerializeField] private GameObject gripPrompt;

    [Header("Debug")]
    [SerializeField] private bool debugGrip = true;

    public bool IsHeld { get; private set; }
    public float CurrentStrokeSpeed { get; private set; }
    public float StrokeInput { get; private set; }

    private Vector3 lastHandPosition;

    private Vector3 visualLocalPosition;
    private Quaternion visualLocalRotation;

    // GripPoint position relative to paddle root.
    private Vector3 paddleToGripLocal;

    private bool gripPromptActive;
    private bool previousGripPressed;

    // Saved paddle pose before grab.
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;
    private bool hasSavedPose;

    // ============================================================
    // BOAT MOTION COMPENSATION
    // ============================================================

    // Boat pose recorded during the previous FollowHand().
    private Vector3 previousBoatPosition;
    private Quaternion previousBoatRotation;

    private bool hasPreviousBoatPose;

    private void Awake()
    {
        if (gripPoint == null)
            gripPoint = transform;

        if (visual != null)
        {
            visualLocalPosition =
                transform.InverseTransformPoint(
                    visual.position);

            visualLocalRotation =
                Quaternion.Inverse(transform.rotation) *
                visual.rotation;
        }

        paddleToGripLocal =
            gripPoint.localPosition;

        previousGripPressed =
            GetGripPressed();
    }

    private void Update()
    {
        if (boatMode != null &&
            !boatMode.IsInBoat)
        {
            if (IsHeld)
                Release();

            StrokeInput = 0f;
            CurrentStrokeSpeed = 0f;

            SetGripPrompt(false);

            previousGripPressed =
                GetGripPressed();

            return;
        }

        bool gripPressed =
            GetGripPressed();

        bool handNearGrip =
            IsHandNearGrip();

        gripPromptActive =
            !IsHeld &&
            handNearGrip;

        SetGripPrompt(
            gripPromptActive);

        bool gripDownThisFrame =
            gripPressed &&
            !previousGripPressed;

        if (!IsHeld)
        {
            StrokeInput = 0f;
            CurrentStrokeSpeed = 0f;

            if (gripPromptActive &&
                gripDownThisFrame)
            {
                Grab();
            }
        }
        else
        {
            if (gripDownThisFrame)
            {
                Release();
            }
            else
            {
                FollowHand();
            }
        }

        previousGripPressed =
            gripPressed;
    }

    private bool IsHandNearGrip()
    {
        if (handController == null ||
            gripPoint == null)
        {
            return false;
        }

        return Vector3.Distance(
                   handController.position,
                   gripPoint.position)
               <= grabDistance;
    }

    private void SetGripPrompt(bool visible)
    {
        if (gripPrompt != null)
            gripPrompt.SetActive(visible);
    }

    private bool GetGripPressed()
    {
        Gamepad gamepad =
            Gamepad.current;

        if (gamepad != null)
        {
            if (handSide == HandSide.Left)
                return gamepad.buttonWest.isPressed;

            return gamepad.buttonSouth.isPressed;
        }

        if (handSide == HandSide.Left)
        {
            return leftHand != null &&
                   leftHand.GripPressed;
        }

        return rightHand != null &&
               rightHand.GripPressed;
    }

    private void Grab()
    {
        if (!gripPromptActive ||
            handController == null ||
            gripPoint == null)
        {
            return;
        }

        // Save exact paddle pose before grab.
        savedLocalPosition =
            transform.localPosition;

        savedLocalRotation =
            transform.localRotation;

        hasSavedPose = true;

        IsHeld = true;

        lastHandPosition =
            handController.position;

        // Initialize boat compensation.
        CaptureCurrentBoatPose();

        BeginHandPaddleHold();

        paddleToGripLocal =
            gripPoint.localPosition;

        if (debugGrip)
        {
            Debug.Log(
                $"[BoatPaddle {handSide}] GRAB",
                this);
        }

        FollowHand();
    }

    private void Release()
    {
        if (!IsHeld)
            return;

        IsHeld = false;

        CurrentStrokeSpeed = 0f;
        StrokeInput = 0f;

        EndHandPaddleHold();

        if (hasSavedPose)
        {
            transform.localPosition =
                savedLocalPosition;

            transform.localRotation =
                savedLocalRotation;

            hasSavedPose = false;
        }

        hasPreviousBoatPose = false;

        if (debugGrip)
        {
            Debug.Log(
                $"[BoatPaddle {handSide}] RELEASE",
                this);
        }

        ApplyVisual();
    }

    private void BeginHandPaddleHold()
    {
        if (handSide == HandSide.Left)
        {
            if (leftHand != null)
                leftHand.BeginPaddleHold();
        }
        else
        {
            if (rightHand != null)
                rightHand.BeginPaddleHold();
        }
    }

    private void EndHandPaddleHold()
    {
        if (handSide == HandSide.Left)
        {
            if (leftHand != null)
                leftHand.EndPaddleHold();
        }
        else
        {
            if (rightHand != null)
                rightHand.EndPaddleHold();
        }
    }

    // ============================================================
    // FOLLOW HAND
    // ============================================================

    private void FollowHand()
    {
        if (handController == null ||
            gripPoint == null)
        {
            return;
        }

        float dt =
            Mathf.Max(
                Time.deltaTime,
                0.0001f);

        // --------------------------------------------------------
        // QUAN TRỌNG:
        //
        // Không dùng:
        //
        // handPosition - lastHandPosition
        //
        // vì phần đó bao gồm cả chuyển động của THUYỀN.
        //
        // Ta biến lastHandPosition từ pose của thuyền FRAME TRƯỚC
        // sang pose hiện tại của thuyền.
        //
        // Phần còn lại mới là chuyển động thực sự của tay
        // tương đối với thuyền.
        // --------------------------------------------------------

        Vector3 boatCompensatedLastHand =
            GetBoatCompensatedPreviousHandPosition();

        Vector3 relativeDelta =
            handController.position -
            boatCompensatedLastHand;

        // --------------------------------------------------------
        // STROKE
        // --------------------------------------------------------

        CurrentStrokeSpeed =
            relativeDelta.magnitude / dt;

        Vector3 strokeDirection =
            GetStrokeDirectionWorld();

        StrokeInput =
            Vector3.Dot(
                relativeDelta / dt,
                strokeDirection);

        // --------------------------------------------------------
        // MOVE PADDLE
        // --------------------------------------------------------

        if (oarPivot != null)
        {
            FollowWithMechanicalPivot();
        }
        else
        {
            transform.rotation =
                handController.rotation *
                Quaternion.Inverse(
                    gripPoint.localRotation);

            Vector3 gripOffset =
                transform.TransformVector(
                    gripPoint.localPosition);

            transform.position =
                handController.position -
                gripOffset;
        }

        ApplyVisual();

        // --------------------------------------------------------
        // STORE CURRENT STATE
        // --------------------------------------------------------

        lastHandPosition =
            handController.position;

        CaptureCurrentBoatPose();
    }

    // ============================================================
    // BOAT COMPENSATION
    // ============================================================

    private Vector3 GetBoatCompensatedPreviousHandPosition()
    {
        Transform boat =
            GetBoatTransform();

        if (boat == null ||
            !hasPreviousBoatPose)
        {
            return lastHandPosition;
        }

        // previousBoatPosition / rotation mô tả hệ quy chiếu
        // của thuyền ở frame trước.
        //
        // Chuyển lastHandPosition về local space của thuyền
        // ở frame trước...
        Vector3 previousBoatLocalPoint =
            Quaternion.Inverse(previousBoatRotation) *
            (lastHandPosition -
             previousBoatPosition);

        // ...sau đó đưa chính điểm đó sang pose hiện tại.
        Vector3 currentBoatPoint =
            boat.position +
            boat.rotation *
            previousBoatLocalPoint;

        return currentBoatPoint;
    }

    private void CaptureCurrentBoatPose()
    {
        Transform boat =
            GetBoatTransform();

        if (boat == null)
        {
            hasPreviousBoatPose = false;
            return;
        }

        previousBoatPosition =
            boat.position;

        previousBoatRotation =
            boat.rotation;

        hasPreviousBoatPose = true;
    }

    private Transform GetBoatTransform()
    {
        if (boatMode != null)
            return boatMode.transform;

        return null;
    }

    // ============================================================
    // MECHANICAL PIVOT
    // ============================================================

    private void FollowWithMechanicalPivot()
    {
        // --------------------------------------------------------
        // STEP 1
        // Rotate ONLY around the support axis.
        // --------------------------------------------------------

        Vector3 pivotToHand =
            handController.position -
            oarPivot.position;

        Vector3 currentGripWorld =
            transform.TransformPoint(
                paddleToGripLocal);

        Vector3 pivotToGrip =
            currentGripWorld -
            oarPivot.position;

        Vector3 pivotAxis =
            oarPivot.up;

        Vector3 projectedHand =
            Vector3.ProjectOnPlane(
                pivotToHand,
                pivotAxis);

        Vector3 projectedGrip =
            Vector3.ProjectOnPlane(
                pivotToGrip,
                pivotAxis);

        if (projectedHand.sqrMagnitude >
            0.000001f &&
            projectedGrip.sqrMagnitude >
            0.000001f)
        {
            Quaternion rotationDelta =
                Quaternion.FromToRotation(
                    projectedGrip.normalized,
                    projectedHand.normalized);

            transform.rotation =
                rotationDelta *
                transform.rotation;
        }

        // --------------------------------------------------------
        // STEP 2
        // Make GripPoint exactly follow the hand.
        // --------------------------------------------------------

        currentGripWorld =
            transform.TransformPoint(
                paddleToGripLocal);

        transform.position +=
            handController.position -
            currentGripWorld;
    }

    private void ApplyVisual()
    {
        if (visual == null)
            return;

        visual.position =
            transform.TransformPoint(
                visualLocalPosition);

        visual.rotation =
            transform.rotation *
            visualLocalRotation;
    }

    public Vector3 GetStrokeDirectionWorld()
    {
        return transform
            .TransformDirection(
                localStrokeAxis)
            .normalized;
    }

    public float GetStrokeInput()
    {
        return IsHeld
            ? StrokeInput
            : 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (gripPoint != null)
        {
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                gripPoint.position,
                grabDistance);

            Gizmos.color =
                Color.green;

            Gizmos.DrawSphere(
                gripPoint.position,
                0.1f);
        }

        if (oarPivot != null)
        {
            Gizmos.color =
                Color.cyan;

            Gizmos.DrawSphere(
                oarPivot.position,
                0.03f);

            Gizmos.DrawLine(
                oarPivot.position,
                oarPivot.position +
                oarPivot.up * 0.20f);
        }
    }
}
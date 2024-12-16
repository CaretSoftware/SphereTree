using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

public class Gizmos3D {
    private float size = 5f;

#if UNITY_EDITOR
    //private void DrawFOV(Vector3 offset, float fovAngle, float visionRadius, float cylinderHeight, Color color) {
    //    Transform myTransform = transform;
    //    Color transparent = new Color(color.r, color.g, color.b, .25f);
//
    //    Vector3 transformPosition = myTransform.position + offset;
    //    Vector3 forward = myTransform.forward;
//
    //    Quaternion rotation = Quaternion.Euler(0f, -fovAngle * .5f, 0f);
    //    Vector3 rotatedForward = rotation * forward;
//
    //    Quaternion rotation2 = Quaternion.Euler(0f, fovAngle * .5f, 0f);
    //    Vector3 rotatedForward2 = rotation2 * forward;
//
    //    Vector3 yOffset = Vector3.up * offset.y;
    //    
    //    Handles.zTest = CompareFunction.GreaterEqual;   // Behind objects
    //    DrawCylinder(transparent);
//
    //    Handles.zTest = CompareFunction.LessEqual;      // Visible behind objects
    //    DrawCylinder(color);
    //    
    //    void DrawCylinder(Color currColor) {
    //        // Vision radius
    //        Handles.color = transparent;
    //        Handles.DrawSolidArc(transformPosition - yOffset, Vector3.up, rotatedForward, fovAngle, visionRadius);
//
    //        // Hearing radius
    //        Handles.color = transparent;
    //        Handles.DrawSolidArc(transformPosition - yOffset, Vector3.up, rotatedForward2, 360 - fovAngle, visionRadius * .5f);
//
    //        Handles.color = currColor;
    //        Cylinder(transformPosition, visionRadius, cylinderHeight, 2f);
    //    }
    //}
    
    private void OnDrawGizmos() {
        // Sphere
        Handles.color = Color.white;
        Sphere(Vector3.zero, size, 5f);
    }

    public static void Cylinder(Vector3 baseCenter, float radius, float height, float lineThickness = 1f) {
        Color currColor = Handles.color;
        Color opacity = currColor;
        opacity.a *= .15f;
        Vector3 heightVector = Vector3.up * height;
        Vector3 left =  CircleEdgeToCameraPerspective(baseCenter, radius, false);
        Vector3 right = CircleEdgeToCameraPerspective(baseCenter, radius, true);
        float cameraHeight = Camera.current.transform.position.y;
        
        Handles.DrawLine(left, left + heightVector, lineThickness);
        Handles.DrawLine(right, right + heightVector, lineThickness);
        
        //Handles.DrawWireDisc(baseCenter, Vector3.up, radius, lineThickness);

        float angle = Vector3.Angle(left - baseCenter, right - baseCenter);
        Handles.DrawWireArc(baseCenter, Vector3.up, right - baseCenter, angle, radius, lineThickness);
        Handles.DrawWireArc(baseCenter + heightVector, Vector3.up, right - baseCenter, angle, radius, lineThickness);

        Handles.color = cameraHeight > baseCenter.y ? opacity : currColor;
        Handles.DrawWireArc(baseCenter, Vector3.up, left - baseCenter, 360f - angle, radius, lineThickness);
        Handles.color = cameraHeight < baseCenter.y + heightVector.y ? opacity : currColor;
        Handles.DrawWireArc(baseCenter + heightVector, Vector3.up, left - baseCenter, 360f - angle, radius, lineThickness);
        Handles.color = currColor;
    }
    
    public static void Capsule(Vector3 baseCenter, float radius, float height, float lineThickness = 1f) {
        height = Mathf.Clamp(height, radius * 2f, height);
        float sideHeight = height - radius * 2;
        Vector3 sphereCenterBottom = baseCenter + Vector3.up * radius;
        Vector3 sphereCenterTop = baseCenter + Vector3.up * height - Vector3.up * radius;
        Vector3 heightVector = Vector3.up * sideHeight;
        Color currColor = Handles.color;
        Color opacity = currColor;
        opacity.a *= .15f;
        
        Vector3 left =  CircleEdgeToCameraPerspective(sphereCenterBottom, radius, false);
        Vector3 right = CircleEdgeToCameraPerspective(sphereCenterBottom, radius, true);
        float angle = Vector3.Angle(left - sphereCenterBottom, right - sphereCenterBottom);
        Handles.DrawWireArc(sphereCenterBottom, Vector3.up, right - sphereCenterBottom, angle, radius, lineThickness);
        Handles.DrawWireArc(sphereCenterTop, Vector3.up, right - sphereCenterBottom, angle, radius, lineThickness);
        
        Handles.color = opacity;
        Handles.DrawWireArc(sphereCenterBottom, Vector3.up, left - sphereCenterBottom, 360f - angle, radius, lineThickness);
        Handles.DrawWireArc(sphereCenterTop, Vector3.up, left - sphereCenterBottom, 360f - angle, radius, lineThickness);
        Handles.color = currColor;

        Sphere(sphereCenterBottom, radius, lineThickness);
        Sphere(sphereCenterTop, radius, lineThickness);
        
        Handles.DrawLine(left, left + heightVector, lineThickness);
        Handles.DrawLine(right, right + heightVector, lineThickness);
    }

    public static void Sphere(Transform objectTransform, float radius, float lineThickness = 1f) =>
        Sphere(objectTransform.localToWorldMatrix, radius, lineThickness);

    public static void Sphere(Vector3 position, float radius, float lineThickness = 1f) =>
        Sphere(Matrix4x4.TRS(position, Quaternion.identity, Vector3.one), radius, lineThickness);

    //https://discussions.unity.com/t/drawing-a-camera-facing-circle-like-unitys-spherecollider-gizmo/205213/2
    public static void Sphere(Matrix4x4 localToWorldMatrix, float radius, float lineThickness = 1f) {
        if (Camera.current.orthographic) {
            Vector3 normal = CameraFwd();
            Vector3 pos = localToWorldMatrix.GetPosition();
            Handles.DrawWireDisc(pos, normal, radius, lineThickness);
            Handles.DrawSolidDisc(pos, normal, .1f);
        } else {
            Matrix4x4 lastMatrix = Handles.matrix;
            Handles.matrix = localToWorldMatrix;
            Vector3 normal = Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
            float sqrMagnitude = normal.sqrMagnitude;
            float num0 = radius * radius;
            float num1 = num0 * num0 / sqrMagnitude;
            float num2 = Mathf.Sqrt(num0 - num1);
            Handles.DrawWireDisc(num0 * normal / sqrMagnitude, normal, num2, lineThickness);
            Handles.matrix = lastMatrix;
            Handles.DrawSolidDisc(localToWorldMatrix.GetPosition(), normal, lineThickness * 0.15f);
        }
    }
    
    public static void WireArc(Transform objectTransform, float radius, float lineThickness) =>
        WireArc(objectTransform.localToWorldMatrix, radius, lineThickness);

    public static void WireArc(Vector3 position, float radius, float lineThickness) =>
        WireArc(Matrix4x4.TRS(position, Quaternion.identity, Vector3.one), radius, lineThickness);
    
    public static void WireArc(Matrix4x4 localToWorldMatrix, float radius, float lineThickness = 1f) {
        if (Camera.current.orthographic) {
            Vector3 normal = CameraFwd();
            Handles.DrawWireDisc(localToWorldMatrix.GetPosition(), normal, radius, lineThickness);
        } else {
            Matrix4x4 lastMatrix = Handles.matrix;
            Handles.matrix = localToWorldMatrix;
            Vector3 normal = Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
            float sqrMagnitude = normal.sqrMagnitude;
            float num0 = radius * radius;
            float num1 = num0 * num0 / sqrMagnitude;
            float num2 = Mathf.Sqrt(num0 - num1);
            Handles.DrawWireDisc(num0 * normal / sqrMagnitude, normal, num2, lineThickness);
            Handles.matrix = lastMatrix;
        }
    }
    
#region HelperFunctions

    private static Vector3 CircleEdgeToCameraPerspective(Vector3 position, float radius, bool right) {
        Vector3 direction = right ? Vector3.right * radius : Vector3.left * radius;

        if (Camera.current.orthographic) {
            Vector3 dir = Camera.current.transform.TransformDirection(direction);
            return position + dir;
        } else {
            Transform cameraTransform = Camera.current.transform;
            Vector3 cameraDirection = Vector3.ProjectOnPlane(position - cameraTransform.position, Vector3.up);
            float cameraDistance = cameraDirection.magnitude;
            float angle = Mathf.Asin(radius / cameraDistance); // α = arc sin (opposite / hypotenuse)
            Quaternion perspectiveRotation = quaternion.Euler(0f, angle * (right ? 1 : -1), 0f);
            Quaternion cameraDirectionRotation = 
                    quaternion.LookRotation(
                            Vector3.ProjectOnPlane(cameraDirection.normalized, Vector3.up), 
                            Vector3.up);
            
            return position + cameraDirectionRotation * perspectiveRotation * direction;
        }
    }

    private static Vector3 CameraDir(Vector3 pos) => pos - Camera.current.transform.position;
    
    private static Vector3 CameraFwd() => Camera.current.transform.forward;
    
#endregion    
    
#endif
}

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SCOrderData))]
public class SCOrderDataEditor : Editor
{
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        SCOrderData order = (SCOrderData)target;

        if (order == null)
            return;

        // Eðer deliveryPosition (0,0,0) ise uyarý ver
        if (order.deliveryPosition == Vector3.zero)
        {
            Handles.color = Color.red;
            Handles.Label(Vector3.zero, "DÝKKAT: Teslimat pozisyonu (0,0,0) olarak ayarlý!");
            return;
        }

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(order.deliveryPosition, Vector3.up, 0.5f);

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(order.deliveryPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(order, "Move Delivery Position");
            order.deliveryPosition = newPosition;
            EditorUtility.SetDirty(order);
        }

        Handles.Label(order.deliveryPosition + Vector3.up * 2,
                     $"Sipariþ: {order.orderName}\nPozisyon: {order.deliveryPosition}");
    }
}
#endif

using UnityEngine;

public class AxisDragAndDropHandler : ObjectSelector
{
    private bool isXZMode = true;
    private Plane xzPlane;
    private float initialY;
    private Vector3 screenPoint;
    private Vector3 initialPosition;
    private GameObject wallRight;
    private GameObject wallLeft;
    private GameObject wallBack;
    private GameObject ceiling;
    private GameObject floor;

    void Start()
    {
        xzPlane = new Plane(Vector3.up, Vector3.zero);
        wallRight = GameObject.FindGameObjectWithTag("WallRight");
        wallLeft = GameObject.FindGameObjectWithTag("WallLeft");
        wallBack = GameObject.FindGameObjectWithTag("WallBack");
        ceiling = GameObject.FindGameObjectWithTag("Ceiling");
        floor = GameObject.FindGameObjectWithTag("Floor");

        // 初期状態をXZ軸モードに設定
        OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDrop);
        Debug.Log("初期状態：XZ軸モードをオンにしました");
    }

    public void ToggleAxisMode()
    {
        OperationModeManager.OperationMode currentMode = OperationModeManager.Instance.GetCurrentMode();

        if (currentMode != OperationModeManager.OperationMode.AxisDragAndDrop)
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDrop);
            isXZMode = true;
            Debug.Log("XZ軸モードをオンにしました");
        }
        else
        {
            isXZMode = !isXZMode;
            Debug.Log(isXZMode ? "XZ軸モードに切り替えました" : "XY軸モードに切り替えました");
        }
    }

    void Update()
    {
        if (OperationModeManager.Instance.GetCurrentMode() != OperationModeManager.OperationMode.AxisDragAndDrop) return;

        if (Input.GetMouseButtonDown(0))
        {
            SelectObject();
        }

        if (selectedObject != null && Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (isXZMode)
            {
                // XZ平面での移動（既存のコード）
                if (xzPlane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    Vector3 newPosition = new Vector3(hitPoint.x, selectedObject.transform.position.y, hitPoint.z);
                    if (!IsColliding(newPosition))
                    {
                        selectedObject.transform.position = newPosition;
                    }
                }
            }
            else
            {
                // XY平面での移動（新しいコード）
                Plane xyPlane = new Plane(Camera.main.transform.forward, selectedObject.transform.position);
                if (xyPlane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    Vector3 newPosition = new Vector3(hitPoint.x, hitPoint.y, selectedObject.transform.position.z);
                    if (!IsColliding(newPosition))
                    {
                        selectedObject.transform.position = newPosition;
                    }
                }
            }
        }
    }

    Vector3 CalculateNewPosition()
    {
        return isXZMode ? CalculateXZPosition() : CalculateYPosition();
    }

    Vector3 CalculateXZPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (xzPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 newPosition = ray.GetPoint(rayDistance);
            newPosition.y = initialY;
            return newPosition;
        }
        return selectedObject.transform.position;
    }

    Vector3 CalculateYPosition()
    {
        float screenX = screenPoint.x;
        float screenY = Input.mousePosition.y;
        float screenZ = screenPoint.z;

        Vector3 currentScreenPoint = new Vector3(screenX, screenY, screenZ);
        return Camera.main.ScreenToWorldPoint(currentScreenPoint);
    }

    private bool IsColliding(Vector3 newPosition)
    {
        if (selectedObject == null) return false;

        Bounds objectBounds = GetObjectBounds(selectedObject, newPosition);

        // 部屋の境界との衝突チェック
        if (wallRight != null && objectBounds.max.x > wallRight.transform.position.x) return true;
        if (wallLeft != null && objectBounds.min.x < wallLeft.transform.position.x) return true;
        if (wallBack != null && objectBounds.max.z > wallBack.transform.position.z) return true;
        if (ceiling != null && objectBounds.max.y > ceiling.transform.position.y) return true;

        // 床との衝突チェック（オブジェクトを床の上に配置）
        if (floor != null)
        {
            float floorY = floor.transform.position.y;
            if (objectBounds.min.y < floorY)
            {
                float adjustment = floorY - objectBounds.min.y;
                selectedObject.transform.position = new Vector3(newPosition.x, newPosition.y + adjustment, newPosition.z);
                return false; // 床に接地させたので、衝突としては扱わない
            }
        }

        // 他のオブジェクトとの衝突チェック
        Collider[] hitColliders = Physics.OverlapBox(objectBounds.center, objectBounds.extents, selectedObject.transform.rotation);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != selectedObject && hitCollider.gameObject != floor)
            {
                return true;
            }
        }

        return false;
    }

    private Bounds GetObjectBounds(GameObject obj, Vector3 position)
    {
        Bounds bounds = new Bounds(position, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // オブジェクトの現在位置と新しい位置の差分を計算
        Vector3 offset = position - obj.transform.position;
        bounds.center += offset;

        return bounds;
    }
}


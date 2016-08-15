using UnityEngine;
using System.Collections;

public class _camera : MonoBehaviour {

	static public Vector3 V3 = new Vector3(0,0,0);	//ex : 攝影機看的位置
	float angle_x = 0;	//ex : 攝影機看目標的角度(X軸)
	float angle_y = 0;	//ex : 攝影機看目標的角度(Y軸)

	private float distance = 500;	//ex : 攝影機跟目標的距離
	public float x_speed = 50;		//ex : 當要旋轉攝影機x軸角速時的速度
	public float y_speed = 50;		//ex : 當要旋轉攝影機Y軸角速時的速度
	static public int z_speed = 1;	//ex : 攝影機拉近或是拉遠目標的速度
	public float Mxa_z = 100;
	public float Min_z = 10;

	private Camera m_camera;

	public GameObject ball;			//ex : 當按鍵盤中鍵表示原點的球

	public GameObject Origin_Sphere;

	static public Vector3 Default_View = new Vector3(0,0,0);	//ex : 中心還原點 在沒有任何gcode時座標為( 0 , 0 , 0 ) 有gocde時座標為所有gcode座標的總合平均值

	// Use this for initialization
	void Start ()
	{
		V3 = new Vector3(0,0,0);
		angle_x = 3.141592f;//1.570796f;
		angle_y = 1.57f;//0.523599f;

		m_camera = camera;
	}

	// Update is called once per frame
	void Update ()
	{
		if(Input.GetMouseButton(1))	//ex : 當按下右鍵
		{
			//ex : 改變x軸角度
			angle_x -= Input.GetAxis ("Mouse X") * x_speed  / 600;
			if (angle_x >= 6.28f)
			{
				angle_x = 0;
			}
			else if(angle_x < 0)
			{
				angle_x = 6.28f + angle_x;
			}

			//ex : 改變Y軸角度
			angle_y -= Input.GetAxis ("Mouse Y") * y_speed  / 600;
			if (angle_y >= 1.57f)
			{
				angle_y = 1.57f;
			}
			else if(angle_y < -1.57f)
			{
				angle_y = -1.57f;
			}
		}
		if (_GUI.gui_check)//ex : 沒碰到別的GUI視窗
		{
			//攝影機拉近或是拉遠目標的邏輯
			m_camera.orthographicSize += Input.GetAxis ("Mouse ScrollWheel") * z_speed * Time.deltaTime * 10000;
			if(m_camera.orthographicSize <2)	//ex : 距離若是小於2則等於2
				m_camera.orthographicSize = 2;
		}
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))	//ex : 若是按下左shift或是右shift
		{
			if(Input.GetMouseButton(2))	//ex : 以及按下滑鼠中鍵...則改變攝影機觀看目標的y軸位置
			{
				float temp_y = Input.GetAxis ("Mouse Y") * 10;
				V3 -= new Vector3(0,temp_y,0);

				ball.renderer.enabled = true;
			}
			else
			{
				ball.renderer.enabled = false;
			}
		}
		else
		{
			if(Input.GetMouseButton(2))	//ex : 若只是按下滑鼠中鍵...則改變滑鼠觀看目標的X軸和z軸座標
			{
				Transform camera_m = camera.transform;//Camera.main.transform;		//取得主要攝影機的matrix(座標-方向-大小)
				Vector3 forward = camera_m.TransformDirection(Vector3.forward);		//取得攝影機觀看的向量
				forward.y = 0;
				forward = forward.normalized;
				
				Vector3 right = new Vector3(forward.z, 0, -forward.x);	//取得攝影機觀看向量的向右向量

				V3 -= Input.GetAxis ("Mouse Y") * forward * 10;
				V3 -= Input.GetAxis ("Mouse X") * right * 10;

				ball.renderer.enabled = true;
			}
			else
			{
				ball.renderer.enabled = false;
			}
		}

		//ex : 持續要求攝影機觀看v3座標
		transform.position = V3 + new Vector3( Mathf.Cos(angle_x) * -distance * Mathf.Cos(angle_y), Mathf.Sin(angle_y) * distance , Mathf.Sin(angle_x) * -distance * Mathf.Cos(angle_y));
		transform.rotation = Quaternion.LookRotation(V3 - transform.position);

		ball.transform.position = V3;

		if (Input.GetKeyUp (KeyCode.Z))
		{
			V3 = Default_View;	//ex : 若是按下鍵盤z則將v3重設為中心還原點
		}

		if (Input.GetKey (KeyCode.O))	//ex : 當按下o則工作原點會追隨滑鼠位置
		{
			Ray mouse_ray = Camera.main.ScreenPointToRay(Input.mousePosition);		//將滑鼠點螢幕的座標轉換為世界座標
			
			if(mouse_ray.direction.y <= 0)
			{
				//以下是將滑鼠的2D座標轉換為3D座標
				float t = -mouse_ray.origin.y / mouse_ray.direction.y;
				Vector3 O_v3 = mouse_ray.origin + (t * mouse_ray.direction);
				O_v3.y = -0.55f;
				Origin_Sphere.transform.position = O_v3;
			}
		}
	}
}

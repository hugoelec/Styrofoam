using UnityEngine;
using System.Collections;

using System.Text.RegularExpressions; //為了讓TextField只有數字

//using UnityEditor;


using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;

public class _GUI : MonoBehaviour {

	private string str_number;	//ex : 用來暫存字串...以方便將字串予數字間做轉換


	static public string importFilePath = "";	//ex : 讀檔路徑

	static public string SaveFilePath = "";		//ex : 存檔路徑

	static public string old_file = "";

//	const int menu_sw = 2;
//	string[] name = new string[menu_sw];


	//
	private Vector2 scrollPosition , scrollPosition1;    //用於拉條
	private Rect scrollSize , realSize;  //表單視窗參數	//ex : 用來產生視窗的拉霸
	static public bool[,] _id = new bool[2,1000];		//ex : 物件選單...用來儲存哪些路線已經被使用者勾選
//	int Lock_Num = -1;
	//
	float setting_open = 0;	//ex : 因為打開設定檔視窗時有一秒時間的 "由小漸大" 展開~因此這值便是由0到1的數據變化
	bool setting_show;		//ex : 設定檔視窗是否展開
	bool about_show;		//ex : 聯絡我們 視窗是否展開

	//

	Rect List , List1 ;

	static public bool gui_check;


	static public string[,] List_order = new string[2,1000];
	static public int[] List_order_index = new int[2];
	static public int[,] List_type_num = new int[2,1000];
	// Use this for initialization
	void Start ()
	{
		setting_show = false;			//ex : 不顯示設定檔視窗
		about_show = false;				//ex : 聯絡我們 視窗關閉

		for (int i=0; i<1000; i++)		//ex : 將物件選單清空
		{
			for(int j = 0 ; j<2 ; j++)
			{
				_id[j,i] = false;
			}
		}
	//	Lock_Num = -1;

	//	name[0] = "Full";
	//	name[1] = "Half";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private int check1 , check2;
	bool UP1_buf , DOWN1_buf , UP2_buf , DOWN2_buf ;
	
//	private bool one_time = true;
	void OnGUI()
	{
		if (setting_show)	//ex : 如果要顯示設定檔視窗 
		{
			GUI.Window(3,new Rect(UnityEngine.Screen.width/2*(1-setting_open),UnityEngine.Screen.height/2*(1-setting_open),UnityEngine.Screen.width*setting_open,UnityEngine.Screen.height*setting_open) ,setting,"Setting");

			if(setting_open >= 1)	//ex : 數值大1則等於1
			{
				setting_open = 1;
			}
			else
			{
				setting_open += Time.deltaTime*2;	//ex : 累加
			}
		}
		else
		{
			setting_open = 0;	//ex : 沒展開設定檔視窗時則把數值歸0
		}

		if(setting_open != 1)	//ex : 設定檔視窗尚未完全展開時
		{
			scrollSize = new Rect(0,20,190,UnityEngine.Screen.height-25);
			realSize = new Rect (0, 0, 170, (Load_TXT.points_num + Load_TXT.line_num + Load_TXT.path_num)*20);
			List = new Rect(UnityEngine.Screen.width-195,0,195,UnityEngine.Screen.height);
			GUI.Window(1,List ,showList,"Final List");    //創造一個2D視窗裡面執行showList()

			if(Input.GetKey(KeyCode.Space))	//ex : 當有按下 空白鍵
			{
				List1 = new Rect(UnityEngine.Screen.width-400,0,195,UnityEngine.Screen.height);
				GUI.Window(2,List1 ,showList1,"Priority List");	//創造一個2D視窗裡面執行showList1()
			}

			if(List.Contains(Input.mousePosition))//檢查滑鼠的座標是否有在Rect矩陣的範圍內
			{
				gui_check = false;
				if( (Input.GetKey(KeyCode.A)) )		//ex : 當有按下a鍵
				{
					for(int i=0 ; i<List_order_index[0] ; i++)
					{
						_id[0,i] = true;	//ex : 全選路線資料
					}
				}
				if( (Input.GetKey(KeyCode.D)) )		//ex : 當有按下D鍵
				{
					for(int i=0 ; i<List_order_index[0] ; i++)
					{
						_id[0,i] = false;		//ex : 清除所有勾選
					}
				}

				if( (Input.GetKey(KeyCode.UpArrow)) )	//ex : 有按下 "上鍵"
				{
					if(UP1_buf)
					{
						UP1_buf = false;

						string s_buf;
						int i_buf;
						bool b_buf;
						for(int i=1 ; i<List_order_index[0] ; i++)	//ex : 將有被勾選的路線資料上移
						{
							if( (_id[0,i])&&(!_id[0,i - 1]) )
							{
								s_buf = List_order[0 , i-1];
								i_buf = List_type_num[0 , i-1];
								b_buf = _id[0 , i-1];

								List_order[0 , i-1] = List_order[0,i];
								List_type_num[0 , i-1] = List_type_num[0,i];
								_id[0 , i-1] = _id[0,i];

								List_order[0,i] = s_buf;
								List_type_num[0,i] = i_buf;
								_id[0,i] = b_buf;
							}
						}
					}
				}
				else
				{
					UP1_buf = true;
				}

				if( (Input.GetKey(KeyCode.DownArrow)) )		//ex : 有按下 "下鍵"
				{
					if(DOWN1_buf)
					{
						DOWN1_buf = false;
						
						string s_buf;
						int i_buf;
						bool b_buf;
						for(int i=(List_order_index[0] - 2) ; i>=0 ; i--)	//ex : 將有被勾選的路線資料下移
						{
							if( (_id[0,i])&&(!_id[0,i + 1]) )
							{
								s_buf = List_order[0 , i+1];
								i_buf = List_type_num[0 , i+1];
								b_buf = _id[0 , i+1];
								
								List_order[0 , i+1] = List_order[0,i];
								List_type_num[0 , i+1] = List_type_num[0,i];
								_id[0 , i+1] = _id[0,i];
								
								List_order[0,i] = s_buf;
								List_type_num[0,i] = i_buf;
								_id[0,i] = b_buf;
							}
						}
					}
				}
				else
				{
					DOWN1_buf = true;
				}
			}
			else if(List1.Contains(Input.mousePosition) && Input.GetKey(KeyCode.Space))	//ex : 當滑鼠有涵蓋在Priority List視窗範圍內 + 有按下空白鍵
			{
				gui_check = false;
				if( (Input.GetKey(KeyCode.A)) )		//ex : 當有按下a鍵
				{
					for(int i=0 ; i<List_order_index[1] ; i++)
					{
						_id[1,i] = true;	//ex : 全選路線資料
					}
				}
				if( (Input.GetKey(KeyCode.D)) )		//ex : 當有按下D鍵
				{
					for(int i=0 ; i<List_order_index[1] ; i++)
					{
						_id[1,i] = false;		//ex : 清除所有勾選
					}
				}

				if( (Input.GetKey(KeyCode.UpArrow)) )	//ex : 有按下 "上鍵"
				{
					if(UP2_buf)
					{
						UP2_buf = false;
						
						string s_buf;
						int i_buf;
						bool b_buf;
						for(int i=1 ; i<List_order_index[1] ; i++)	//ex : 將有被勾選的路線資料上移
						{
							if( (_id[1,i])&&(!_id[1 , i - 1]) )
							{
								s_buf = List_order[1 , i-1];
								i_buf = List_type_num[1 , i-1];
								b_buf = _id[1 , i-1];
								
								List_order[1 , i-1] = List_order[1 , i];
								List_type_num[1 , i-1] = List_type_num[1,i];
								_id[1 , i-1] = _id[1 , i];
								
								List_order[1 , i] = s_buf;
								List_type_num[1 , i] = i_buf;
								_id[1 , i] = b_buf;
							}
						}
					}
				}
				else
				{
					UP2_buf = true;
				}
				
				if( (Input.GetKey(KeyCode.DownArrow)) )		//ex : 有按下 "下鍵"
				{
					if(DOWN2_buf)
					{
						DOWN2_buf = false;
						
						string s_buf;
						int i_buf;
						bool b_buf;
						for(int i=(List_order_index[1] - 2) ; i>=0 ; i--)	//ex : 將有被勾選的路線資料下移
						{
							if( (_id[1 , i])&&(!_id[1 , i + 1]) )
							{
								s_buf = List_order[1 , i+1];
								i_buf = List_type_num[1 , i+1];
								b_buf = _id[1 , i+1];
								
								List_order[1 , i+1] = List_order[1 , i];
								List_type_num[1 , i+1] = List_type_num[1 , i];
								_id[1 , i+1] = _id[1 , i];
								
								List_order[1 , i] = s_buf;
								List_type_num[1 , i] = i_buf;
								_id[1 , i] = b_buf;
							}
						}
					}
				}
				else
				{
					DOWN2_buf = true;
				}
			}
			else
			{
				gui_check = true;
			}

			//
//			if(Lock_Num != -1)
//			{
//				GUI.Window(2 , new Rect ((UnityEngine.Screen.width/4)-80, UnityEngine.Screen.height-100-20, 160, 100) , list , "Menu");
//			}
			//
			
			if(GUI.Button(new Rect(310, 30, 60, 20),"Save"))//ex : 當按下save按鍵...則開始進行存檔...此button只負責取路徑...正式的存檔動作在Load_TXT.cs
			{
				System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
				if(old_file == "")
				{
					sfd.InitialDirectory ="file://"+UnityEngine.Application.dataPath;//開啟時的預設目錄
				}
				else
				{
					sfd.InitialDirectory = old_file;
				}
				sfd.Filter = "All files (*.*)|*.*|gco files (*.gco)|*.gco" ;
				sfd.FilterIndex = 2 ;//預設副檔名的index
				sfd.RestoreDirectory = true ;
				
				if(sfd.ShowDialog()== System.Windows.Forms.DialogResult.OK)	//ex : 若是有選檔成功
				{
					SaveFilePath = sfd.FileName;	//ex : 得到路徑 + 檔名		如 : c:\123\456.txt

				}
			}

			if(GUI.Button(new Rect(230, 30, 60, 20),"Setting"))	//ex : 開啟設定檔視窗
			{
				setting_show = true;
				mix_error_str = Load_TXT.mix_error.ToString();
				Multiply_float_s = Load_TXT.Multiply_float.ToString();
			}
			
			if(GUI.Button(new Rect(150, 30, 60, 20),"Open"))	//ex : 當按下OPEN按鍵...則開啟讀檔...此button只負責取路徑...正式的讀檔動作在Load_TXT.cs
			{
				//importFilePath = EditorUtility.OpenFilePanel ("" , importFilePath , "gco*");;
				System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog ();//開啟舊檔
				if(old_file == "")
				{
					ofd.InitialDirectory ="file://"+UnityEngine.Application.dataPath;//開啟時的預設目錄
				}
				else
				{
					ofd.InitialDirectory = old_file;
				}
				ofd.Filter = "All files (*.*)|*.*|SVG files (*.SVG)|*.SVG" ;
				ofd.FilterIndex = 2 ;//預設副檔名的index
				ofd.RestoreDirectory = true ;
				
				if(ofd.ShowDialog()== System.Windows.Forms.DialogResult.OK)
				{
					importFilePath = ofd.FileName;
					old_file = Path.GetFileName(importFilePath);	//先取檔名
					old_file = ofd.FileName.Replace(old_file , "");	// 刪除檔名~即可得到路徑

					
					for (int i=0; i<1000; i++)	//ex : 清空資料
					{
						for(int j=0;j<2;j++)
						{
							_id[j,i] = false;
						}
					}
				//	Lock_Num = -1;
					
					Load_TXT.Lock_sw = -1;	//ex : 讀檔旗標開始
				}
			}
			
			////
			
			if (GUI.Button (new Rect (20, UnityEngine.Screen.height - 50, 100, 20), "about"))	//ex : 當按下about按鍵
			{
				about_show = true;	//ex : 設定檔旗標為true
			}
			
			if(about_show)
			{
				GUI.Window(0,new Rect ((UnityEngine.Screen.width/2)-150, (UnityEngine.Screen.height/2)-100, 300, 200),about,"about");    //創造一個2D視窗裡面執行showList()
			}
		}

	}

	void about(int windowID)
	{
		if (GUI.Button (new Rect ( 150-50, 200-40 , 100, 20), "OK"))	//ex : 當按下ok按鍵...則關閉 關於我們 的視窗
		{
			about_show = false;
		}
		GUI.Label (new Rect (10, 30, 280, 25), "Developed by MegJia Inc. (Kaohsiung) all right reserved");
		GUI.Label (new Rect (10, 60, 280, 25), "Tel:+886-963-258-888");
		GUI.Label (new Rect (10, 90, 280, 25), "email: hugoelec@gmail.com");
	}
	void showList(int windowID)	//ex : Final List 表單內容
	{
		realSize = new Rect (0, 0, 170, List_order_index[0]*30);
		scrollPosition = GUI.BeginScrollView (scrollSize, scrollPosition, realSize);//.BeginScrollView(scrollPosition, GUILayout.Width(length[0]+100), GUILayout.Height(Screen.height));	//開始選單拉條

		for(int i=0 ; i<List_order_index[0] ; i++)
		{
			_id[0,i] = GUI.Toggle(new Rect(10,(i*30),150,20), _id[0,i], List_order[0,i] );

			if( Input.GetKey(KeyCode.KeypadPlus) && _id[0,i] )	//ex : 如果按下九宮格數字區的 '+'鑑...則將Final List的資料轉到Priority List內
			{
				List_order[1 , List_order_index[1]] = List_order[0,i];
				List_type_num[1,List_order_index[1]] = List_type_num[0,i];
				List_order_index[1]++;
				for(int j=i ; j<List_order_index[0] ; j++)
				{
					List_order[0,j] = List_order[0,j+1];
					List_type_num[0,j] = List_type_num[0,j+1];
					_id[0,j] = _id[0,j+1];
				}
				List_order_index[0]--;
				i--;
			}
		}
		GUI.EndScrollView();	//結束選單拉條
	}

	void showList1(int windowID)	//ex : Priority List 表單內容
	{
		realSize = new Rect (0, 0, 170, List_order_index[1]*30);
		scrollPosition1 = GUI.BeginScrollView (scrollSize, scrollPosition1, realSize);//.BeginScrollView(scrollPosition1, GUILayout.Width(length[0]+100), GUILayout.Height(Screen.height));	//開始選單拉條
		
		for(int i=0 ; i<List_order_index[1] ; i++)
		{
			_id[1,i] = GUI.Toggle(new Rect(10,(i*30),150,20), _id[1,i], List_order[1,i] );
			
			if( Input.GetKey(KeyCode.KeypadMinus) && _id[1,i] )		//ex : 如果按下九宮格數字區的 '-'鑑...則將Final List的資料轉到Priority List內
			{
				List_order[0 , List_order_index[0]] = List_order[1,i];
				List_type_num[0,List_order_index[0]] = List_type_num[1,i];
				List_order_index[0]++;
				for(int j=i ; j<List_order_index[1] ; j++)
				{
					List_order[1,j] = List_order[1,j+1];
					List_type_num[1,j] = List_type_num[1,j+1];
					_id[1,j] = _id[1,j+1];
				}
				List_order_index[1]--;
				i--;
			}
		}
		
		GUI.EndScrollView();	//結束選單拉條
	}

//	void list(int windowID)
//	{
//		int action = Load_TXT.action [Load_TXT.Lock_sw , Load_TXT.Lock_num];
//		bool c1 , c2;
//
//		for(int i=0; i<menu_sw ;i++)
//		{
//			if(action == i)
//			{
//				c1 = true;
//			}
//			else
//			{
//				c1 = false;
//			}
//			c2 = c1;
//			c1 = GUI.Toggle(new Rect(10,(i*30+20),150,20), c1, name[i]);
//			if(c1 != c2)
//			{
//				if(!c2)
//				{
//					Load_TXT.action[Load_TXT.Lock_sw , Load_TXT.Lock_num] = i;
//				}
//			}
//		}
//	}

	string setting_file = "" ;
	public static string setting_name = "";
	static public bool setting_flag = false;

	static public string xx= "X" , yy = "Y" , zz = "Z";

	static public string setting_str = "";

	string mix_error_str;		//ex : 慎用...假設該連結的兩點並未重疊~可以設定此值改善
	string Multiply_float_s;	//ex : 路線的放大倍數

	static public bool x_reverse = false , y_reverse = false;

	static public bool[] sw_bool = new bool[3];
	void setting(int windowID)	//ex : 設定檔內容
	{
		GUI.Label (new Rect(20,50,100,30),"XY Feed Rate :");
		Load_TXT.XY_FR =  GUI.TextField (new Rect(120,50,100,20),Load_TXT.XY_FR);	//ex : 取得使用者期望的xy軸的FEEDRATE
		Load_TXT.XY_FR = Regex.Replace(Load_TXT.XY_FR, "[^0123456789]", "");	//只留數字

		GUI.Label (new Rect(20,80,100,30),"E Ring Value :");
		Load_TXT.E_Ring_value =  GUI.TextField (new Rect(120,80,100,20),Load_TXT.E_Ring_value);		//ex : 已經無用
		Load_TXT.E_Ring_value = Regex.Replace(Load_TXT.E_Ring_value, "[^0123456789]", "");	//只留數字

		GUI.Label (new Rect(20,110,100,30),"Z Height :");
		Load_TXT.Z_Height =  GUI.TextField (new Rect(120,110,100,20),Load_TXT.Z_Height);	//ex : z軸的變化值...不是每個case都需要
		Load_TXT.Z_Height = Regex.Replace(Load_TXT.Z_Height, "[^0123456789]", "");	//只留數字

		GUI.Label (new Rect(20,140,100,30),"Multiply :");
		Multiply_float_s = GUI.TextField (new Rect(120,140,100,20),Multiply_float_s);	//ex : 路線的放大倍數
		Multiply_float_s = Regex.Replace (Multiply_float_s, "[^0-9.]", "");
	//	string _str =  GUI.TextField (new Rect(120,140,100,20),Load_TXT.Multiply_float.ToString());
	//	Load_TXT.Multiply_float = float.Parse( Regex.Replace(_str, "[^0-9.]", "") );	//只留數字

		GUI.Label (new Rect(20,170,100,30),"Mix error :");
		mix_error_str =  GUI.TextField (new Rect(120,170,100,20),mix_error_str);	//ex : 慎用...假設該連結的兩點並未重疊~可以設定此值改善
		mix_error_str = Regex.Replace (mix_error_str, "[^0-9.]", "");
		float.TryParse(mix_error_str,out Load_TXT.mix_error);

		GUI.Label (new Rect(20,200,100,30),"Zoom :");
		string _str =  GUI.TextField (new Rect(120,200,100,20),_camera.z_speed.ToString());	//ex : 攝影機拉遠拉近的中鍵滾輪速度
		_camera.z_speed = int.Parse( Regex.Replace(_str, "[^0-9]", "") );	//只留數字

		bool _t = sw_bool [0];
		sw_bool[0] = GUI.Toggle (new Rect(250,20,100,30) , sw_bool[0] , "Knife");
		if (sw_bool [0] != _t)	//ex : 選擇了Knife模式
		{
			if(_t)
			{
				sw_bool[0] = true;
			}
			else
			{
				for(int t = 0; t < sw_bool.Length ; t++)
				{
					sw_bool[t] = false;
				}
				sw_bool[0] = true;
			}
		}
		_t = sw_bool [1];
		sw_bool[1] = GUI.Toggle (new Rect(250,50,100,30) , sw_bool[1] , "Plasama");
		if (sw_bool [1] != _t)	//ex : 選擇了Plasama模式
		{
			if(_t)
			{
				sw_bool[1] = true;
			}
			else
			{
				for(int t = 0; t < sw_bool.Length ; t++)
				{
					sw_bool[t] = false;
				}
				sw_bool[1] = true;
			}
		}
		_t = sw_bool [2];
		sw_bool[2] = GUI.Toggle (new Rect(250,80,100,30) , sw_bool[2] , "Styrofoam");
		if (sw_bool [2] != _t)	//ex : 選擇了Styrofoam模式
		{
			if(_t)
			{
				sw_bool[2] = true;
			}
			else
			{
				for(int t = 0; t < sw_bool.Length ; t++)
				{
					sw_bool[t] = false;
				}
				sw_bool[2] = true;
			}
		}

	//	x_reverse = GUI.Toggle (new Rect(20,230,100,30) , x_reverse , x_reverse == true?" 1":" 2");
	//	y_reverse = GUI.Toggle (new Rect(20,260,100,30) , y_reverse , y_reverse == true?" 1":" 2");
	/*	GUI.Label (new Rect(20,230,100,30),"X :");
		xx =  GUI.TextField (new Rect(120,230,100,20),xx);

		GUI.Label (new Rect(20,260,100,30),"Y :");
		yy =  GUI.TextField (new Rect(120,260,100,20),yy);
*/

		if(GUI.Button(new Rect(20, 240, 100, 20),"Load setting txt"))	//ex : 讀取設定檔
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog ();//開啟舊檔
			if(setting_file == "")
			{
				ofd.InitialDirectory ="file://"+UnityEngine.Application.dataPath;//開啟時的預設目錄
			}
			else
			{
				ofd.InitialDirectory = setting_file;
			}
			ofd.Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt" ;
			ofd.FilterIndex = 2 ;//預設副檔名的index
			ofd.RestoreDirectory = true ;
			
			if(ofd.ShowDialog()== System.Windows.Forms.DialogResult.OK)
			{
				setting_name = ofd.FileName;
				setting_file = Path.GetFileName(setting_name);	//先取檔名
				setting_file = ofd.FileName.Replace(setting_file , "");	// 刪除檔名~即可得到路徑
				
				setting_flag = true;

				Load_Setting();	//ex : 開始讀取工作模式
			}
		}

		if(GUI.Button(new Rect(20, 280, 100, 20),"Save setting txt"))	//ex : 儲存設定檔
		{
			
			System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
			if(setting_file == "")
			{
				ofd.InitialDirectory ="file://"+UnityEngine.Application.dataPath;//開啟時的預設目錄
			}
			else
			{
				ofd.InitialDirectory = setting_file;
			}
			ofd.Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt" ;
			ofd.FilterIndex = 2 ;//預設副檔名的index
			ofd.RestoreDirectory = true ;
			
			if(ofd.ShowDialog()== System.Windows.Forms.DialogResult.OK)
			{
				setting_name = ofd.FileName;
				setting_file = Path.GetFileName(setting_name);	//先取檔名
				setting_file = ofd.FileName.Replace(setting_file , "");	// 刪除檔名~即可得到路徑


				if(sw_bool[0])	//ex : Knife模式
				{
					setting_str = "Mode=Knife\r\n";
				}
				else if(sw_bool[1])	//ex : Plasama模式
				{
					setting_str = "Mode=Plasama\r\n";
					setting_str += "continue=\r\n";
					setting_str += Load_TXT.Z_LOW + "\r\n\r\n";
					setting_str += "pause=\r\n";
					setting_str += Load_TXT.Z_HIGH + "\r\n\r\n";
				}
				else if(sw_bool[2])	//ex : Styrofoam模式
				{
					setting_str = "Mode=Styrofoam\r\n";
				}
				else 	//ex : 不該出現於此
				{
					setting_str = "Mode=\r\n";
				}
				setting_str += "XY Feed Rate=" + Load_TXT.XY_FR + "\r\n";
				setting_str += "E Ring Value=" + Load_TXT.E_Ring_value + "\r\n";
				setting_str += "Z Height=" + Load_TXT.Z_Height + "\r\n";
				setting_str += "Multiply=" + Multiply_float_s + "\r\n";
				setting_str += "Mix error=" + mix_error_str + "\r\n";
				setting_str += "Zoom=" + _camera.z_speed.ToString();

				Save_Setting();
			}
		}

		if(GUI.Button(new Rect(UnityEngine.Screen.width/2-50, UnityEngine.Screen.height-80, 100, 50),"Back"))	//ex : 離開 設定檔 視窗
		{
			setting_show = false;
			Load_TXT.Multiply_float = float.Parse(Multiply_float_s);
		}
	}

	int ComboBox(int max_value , string[] _str , Rect _rect)	//ex : 無用
	{
		int return_value = -1;
		for(int i=1;i<=max_value;i++)
		{
			if( GUI.Button(new Rect(_rect.xMin, _rect.yMin+ (i*_rect.height), _rect.width, _rect.height),_str[i-1]) )
			{
				return_value = i-1;
			}
		}
		return return_value;
	}

	void Load_Setting()	//ex : 讀取工作模式
	{
		bool c = false, p = false , s = false;

		string[] setting_File = File.ReadAllLines(_GUI.setting_name);
		int setting_max = setting_File.Length;
		string com_str = "";
		int com_int;
		
		for(int ii = 0 ; ii < setting_max ; ii++)
		{
			string buf = setting_File[ii];
			//	Debug.Log(buf);
			
			com_str = "Mode=";
			com_int = buf.IndexOf(com_str);
			if(com_int != -1)	//ex : 如果此行有出現 "Mode" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				if(buf == "Knife")
				{
					for(int t = 0; t < sw_bool.Length ; t++)
					{
						sw_bool[t] = false;
					}
					sw_bool[0] = true;
				}
				else if(buf == "Plasama")
				{
					for(int t = 0; t < sw_bool.Length ; t++)
					{
						sw_bool[t] = false;
					}
					sw_bool[1] = true;
				}
				else if(buf == "Styrofoam")
				{
					for(int t = 0; t < sw_bool.Length ; t++)
					{
						sw_bool[t] = false;
					}
					sw_bool[2] = true;
				}
			}

			com_str = "XY Feed Rate=";
			com_int = buf.IndexOf(com_str);
			if(com_int != -1)	//ex : 如果此行有出現 "XY Feed Rate=" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				buf = Regex.Replace(buf, "[^0123456789]", "");	//只留數字
				Load_TXT.XY_FR = buf;
			}
			
			com_str = "E Ring Value=";
			com_int = buf.IndexOf(com_str);
			if(com_int != -1)	//ex : 如果此行有出現 "E Ring Value=" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				buf = Regex.Replace(buf, "[^0123456789]", "");	//只留數字
				Load_TXT.E_Ring_value = buf;
			}
			
			com_str = "Z Height=";
			com_int = buf.IndexOf(com_str);	
			if(com_int != -1)	//ex : 如果此行有出現 "Z Height=" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				buf = Regex.Replace(buf, "[^0123456789]", "");	//只留數字
				Load_TXT.Z_Height = buf;
			}
			
			com_str = "Multiply=";
			com_int = buf.IndexOf(com_str);	
			if(com_int != -1)	//ex : 如果此行有出現 "Multiply=" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				buf = Regex.Replace(buf, "[^0123456789.]", "");	//只留數字
				Multiply_float_s = buf;
			}
			
			com_str = "Mix error=";
			com_int = buf.IndexOf(com_str);	
			if(com_int != -1)	//ex : 如果此行有出現 "Mix error=" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				buf = Regex.Replace(buf, "[^0123456789.]", "");	//只留數字
				mix_error_str = buf;
			}
			
			com_str = "Zoom=";
			com_int = buf.IndexOf(com_str);
			if(com_int != -1)	//ex : 如果此行有出現 "Zoom=" 字串
			{
				c = false;
				p = false;
				buf = buf.Substring( com_int + com_str.Length );//, (buf.Length -1 -buf.IndexOf("Mode=")) );
				buf = Regex.Replace(buf, "[^0123456789]", "");	//只留數字
				_camera.z_speed = int.Parse(buf);
			}


			com_str = "continue=";
			com_int = buf.IndexOf(com_str);
			if(com_int != -1)	//ex : 如果此行有出現 "continue=" 字串
			{
				p = false;
				c = true;
				s = true;
			}
			com_str = "pause=";
			com_int = buf.IndexOf(com_str);
			if(com_int != -1)	//ex : 如果此行有出現 "pause=" 字串
			{
				c = false;
				p = true;
				s = true;
			}
			if(c)
			{
				if(s)
				{
					s = false;
					Load_TXT.Z_LOW = "";
				}
				else
				{
					if(buf.Length > 0)
					{
						Load_TXT.Z_LOW += buf + "\r\n";
					}
				}
			}
			if(p)
			{
				if(s)
				{
					s = false;
					Load_TXT.Z_HIGH = "";
				}
				else
				{
					if(buf.Length > 0)
					{
						Load_TXT.Z_HIGH += buf + "\r\n";					
					}
				}
			}
		}
	}

	void Save_Setting()	//ex : 將工作模式儲存於指定路徑
	{
		StreamWriter save_text = new StreamWriter(_GUI.setting_name);
		save_text.WriteLine (setting_str);
		save_text.Close ();
	}
}

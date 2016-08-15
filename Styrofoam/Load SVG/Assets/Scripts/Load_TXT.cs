using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
//using System.Runtime.CompilerServices;
using System.Text.RegularExpressions; //為了讓TextField只有數字

public class Load_TXT : MonoBehaviour {

	protected string text = ""; // assigned to allow first line to be read below
	static public Vector3[] _pos =new Vector3[1000000];
	static public Vector3[] mix_pos =new Vector3[1000000];

	//
	//float Standard = 5;
	//

	protected int i , order , max;

	string[] _File;	//ex : 將讀取的svg檔以每行排列


	private Material lineMaterial;	//ex : 畫線

	static public int sw = -1;

	//player setting
	static public bool _CNC = false;
	static public int[,] action = new int[3,1000];
	static public int[,] work_order = new int[3,1000];
	//

	static public string XY_FR = "300";//"1500";	//ex : xy軸的FEEDRATE
	static public string E_Ring_value = "360";		//ex : 已經無用
	static public string Z_Height = "200";			//ex : z軸的變化值...不是每個case都需要
	static public float Multiply_float = 1.0f;

	private Vector3 fast = new Vector3(0,0,0);
	private bool fast_point = true;

	private const float t_Interval = 100;

	private const float pi = 3.14159f;

	static public int points_num , line_num , path_num , mix_sum , mix_pos_array_index;
	int[] points_index_array = new int[1000] , line_index_array = new int[1000] , path_index_array = new int[1000] , mix_end_array = new int[1000];
	int[] points_start_array = new int[1000] , line_start_array = new int[1000] , path_start_array = new int[1000] , mix_start_array = new int[1000];
	bool[] point_bool = new bool[1000] , line_bool = new bool[1000] , path_bool = new bool[1000];

	static public int Lock_sw = -1 , Lock_num = -1;

	public GameObject Origin_Sphere;
	public GameObject first_point_Sphere;

	static public string Z_HIGH = "G1 Z0.1 F1500" + "\r\n" + "G4 P100" + "\r\n" + "M440" + "\r\n" + "G1 Z0 F1500" + "\r\n" + "G4 P1000" , Z_LOW = "G1 Z0.1 F1500" + "\r\n" + "G4 P100" + "\r\n" + "M441" + "\r\n" + "G1 Z0 F1500" + "\r\n" + "G4 P2000";

	static public float mix_error = 0;

	void Start ()
	{
		points_num = 0;
		line_num = 0;
		path_num = 0;

		i = 0;

		sw = -1;

		if (!lineMaterial)	//ex : 宣告線的屬性
		{
			lineMaterial = new Material ("Shader \"Lines/Colored Blended\" {" +
			                             "SubShader { Pass { " +
			                             "    Blend SrcAlpha OneMinusSrcAlpha " +
			                             "    ZWrite Off Cull Off Fog { Mode Off } " +
			                             "    BindChannels {" +
			                             "      Bind \"vertex\", vertex Bind \"color\", color }" +
			                             "} } }");
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
	bool show_hide = false;
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.H))	//ex : 當按下h鍵 開啟 / 關閉快捷鍵說明
		{
			show_hide = !show_hide;
		}

		if(sw == -1)	//ex : 開始讀檔
		{
			_GUI.importFilePath = "";
			sw = 0;
		}
		else
		{
			if(_GUI.importFilePath.Length != 0)
			{
				for(int k= 0;k<600;k++)
				{
					logic (_GUI.importFilePath);
				}
			}
		}

		////

		if(_GUI.SaveFilePath.Length != 0)	//ex : 存檔路徑不是空的
		{

			Save_gcode(_GUI.SaveFilePath);	//ex : 開始存檔
			_GUI.SaveFilePath = "";		//ex : 把存檔路徑改為空的
		}
		/*
		if (_GUI.setting_name.Length != 0)
		{
			if(_GUI.setting_flag)
			{
				_GUI.setting_flag = false;
				Load_Setting();
			}
			else
			{
				Save_Setting();
			}
			_GUI.setting_name = "";
		}
		*/
	}

	static public float _time = 0;
	Vector3 addr1 , addr2;

	bool t_flag = false;
	bool g_buf = false;

	void OnGUI()
	{
		_time += ( Time.deltaTime);
	//	GUI.Label(new Rect(10, 10, 120, 20),(_time).ToString());
		GUI.Label(new Rect(10, 10, 120, 20),"FPS = " + ((int)(1/Time.deltaTime)).ToString());		//ex : FPS
		GUI.Label(new Rect(10, 30, 120, 20),"click ' H' on / off Hot Key List");
		if(show_hide)
		{
			GUI.Label(new Rect(10, 50, 400, 20),"* 需先到設定裡選擇工作模式");
			GUI.Label(new Rect(10, 70, 400, 20),"* 1 pixel = 1mm");
			GUI.Label(new Rect(10, 90, 400, 20),"* 起始點盡可能靠近原點");
			GUI.Label(new Rect(10, 110, 400, 20),"* 保利龍模式起點盡可能在左上角");
			GUI.Label(new Rect(10, 130, 400, 20),"Mouse Right Button + Mouse Move = Rotate view");
			GUI.Label(new Rect(10, 150, 400, 20),"Mouse Center Button + Mouse Move = Move view on XY plane");
			GUI.Label(new Rect(10, 170, 400, 20),"Shift + Center Mouse Button = Move Vertical View");
			GUI.Label(new Rect(10, 190, 400, 20),"Z = Default View");
			GUI.Label(new Rect(10, 210, 400, 20),"Space = Priority List");
			GUI.Label(new Rect(10, 230, 400, 20),"O = Set Origin Point");
			GUI.Label(new Rect(10, 250, 400, 20),"S = 開啟 / 關閉 全部路徑");
			GUI.Label(new Rect(10, 270, 400, 20),"UP = 在表單上按up則移動選取的路徑");
			GUI.Label(new Rect(10, 290, 400, 20),"DOWN = 在表單上按down則移動選取的路徑");
			GUI.Label(new Rect(10, 310, 400, 20),"A = 在表單上按a則全選");
			GUI.Label(new Rect(10, 330, 400, 20),"D = 在表單上按a則全部取消");
		}

		if((i > 0) && (sw == 0))	//ex : 如果有SVG
		{
			lineMaterial.SetPass( 0 );	//ex : 設定顯示樣子....0 = line
			
			GL.Begin( GL.LINES );	//ex : 開始繪製...需跟下面的GL.End();成雙成對
			Color G_color = new Color(0,0,0);	//ex : 設定顏色

			//

			if(Input.GetKeyDown(KeyCode.S))	//ex : 當按下s鑑
			{
				g_buf = true;
			}
			else if( Input.GetKeyUp(KeyCode.S) )	//ex : 放開s鑑
			{
				if(g_buf)
				{
					g_buf = false;
					t_flag = !t_flag;
				}
			}
			Color B_color = new Color(0,0,0);
			if(t_flag)	//ex : 如果有顯示全部路徑
			{
				GL.Color (B_color);
				GL.Vertex3(Origin_Sphere.transform.position.x , Origin_Sphere.transform.position.y , Origin_Sphere.transform.position.z);
			}

			for(int m = 0 ; m<_GUI.List_order_index[1] ; m++)	//ex : Priority List的線
			{
				if(_GUI._id[1,m])
				{
					G_color = new Color(1,1,0);		//ex : 如果有被勾選的顏色為黃色
				}
				else
				{
					G_color = new Color(1,0,0);		//ex : 如果有未被勾選的顏色為紅色
				}

				for(int n=mix_start_array[_GUI.List_type_num[1,m]] ; n<=mix_end_array[_GUI.List_type_num[1,m]] ; n++)
				{
					if(t_flag)
					{
						GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);	//end
						if( n == mix_end_array[_GUI.List_type_num[1,m]] )
						{
							GL.Color (B_color);
						}
						else
						{
							GL.Color (G_color);
						}
					}
					else
					{
						if( (n != mix_start_array[_GUI.List_type_num[1,m]])&&( n != mix_end_array[_GUI.List_type_num[1,m]]) )
						{
							GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);	//end
						}
						GL.Color (G_color);
					}

					GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);	//start

				}
			}

			for(int m = 0 ; m<_GUI.List_order_index[0] ; m++)	//ex : Final List的線
			{
				if(_GUI._id[0,m])
				{
					G_color = new Color(1,1,0);		//ex : 如果有被勾選的顏色為黃色
				}
				else
				{
					G_color = new Color(1,0,0);		//ex : 如果有未被勾選的顏色為紅色
				}

				for(int n=mix_start_array[_GUI.List_type_num[0,m]] ; n<=mix_end_array[_GUI.List_type_num[0,m]] ; n++)
				{
					if(t_flag)
					{
						GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);	//end
						if( n == mix_end_array[_GUI.List_type_num[0,m]] )
						{
							GL.Color (B_color);
						}
						else
						{
							GL.Color (G_color);
						}
					}
					else
					{
						if( (n != mix_start_array[_GUI.List_type_num[0,m]])&&( n != mix_end_array[_GUI.List_type_num[0,m]]) )
						{
							GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);	//end
						}
						GL.Color (G_color);
					}
					
					GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);	//start
				}
			}
			
			GL.End();	//ex : 畫線結束
		}
		else
		{
			if(sw > 0)	//ex : 讀檔中
			{
				GUIStyle _skin_Label = new GUIStyle(GUI.skin.label);
				//	_skin_Label.font = _font;
				_skin_Label.fontSize = 16;
				_skin_Label.normal.textColor = Color.white;
				_skin_Label.hover.textColor = Color.white;

				GUI.Label(new Rect((UnityEngine.Screen.width/2)-60, (UnityEngine.Screen.height/2)-10, 500, 20),"Loading : "+ order.ToString() +" Lines " + sw.ToString(),_skin_Label);
			}
		}
	}

	Vector3[] logic_V4 = new Vector3[4];
	Vector3 M_source;
	bool path_flag = false;

	float max_x = -1 , max_y = -1;
	void logic(string filename)		//ex : 讀檔邏輯
	{
		switch (sw)
		{
		case 0:
			_File = File.ReadAllLines(filename);	//ex : 一次取一行資料
			max = _File.Length;

			order = 0;

			sw ++;
			i = 0;

			points_num = 0;
			line_num = 0;
			path_num = 0;
			break;
		case 1:
			if (max > order)
			{
				text = _File[order];

				if(text.IndexOf("points=") != -1)	//ex : 如果此筆資料是屬於svg的point則在此分析
				{
					fast_point = true;
					sw = 2;
					text = text.Substring(text.IndexOf("points=")+1,text.Length -1 -text.IndexOf("points="));

					points_start_array[points_num] = i;

					points_num ++;
				}
				else if(text.IndexOf("<line") != -1)	//ex : 如果此筆資料是屬於svg的line則在此分析
				{
					sw = 4;
					text = text.Substring(text.IndexOf("x1")+1,text.Length -1 -text.IndexOf("x1"));
					line_start_array[line_num] = i;

					line_num ++;
				}
				else if(text.IndexOf("path") != -1)	//ex : 如果此筆資料是屬於svg的path則在此分析
				{
					sw = 5;
					text = text.Substring(text.IndexOf("d=")+1,text.Length -1 -text.IndexOf("d="));

					/*path_start_array[path_num] = i;

					path_num ++;*/
					path_flag = false;
				}
				else
				{
					order++;
				}
			}
			else
			{
				sw = 999;
			}
			break;
		case 2:		//ex : 分析point
			if (max > order)
			{
				string[] _temp1 = text.Split('"');
				string[] _temp2 = _temp1[1].Split(' ');

				for(int j=0 ; j < _temp2.Length ; j++)
				{
					string[] _temp3 = _temp2[j].Split(',');

					bool check = false;

					for(int k=0 ; k < _temp3.Length ; k++)	//ex : 將分析的字串轉成數字
					{
						switch(k)
						{
						case 0:
							_temp3[0] = Regex.Replace(_temp3[0], "[^0123456789.]", "");
							if(_temp3[0].Length > 0)
							{
								_pos[i].y = float.Parse( _temp3[0] );
								check = true;
							}
							break;
						case 1:
							_temp3[1] = Regex.Replace(_temp3[1], "[^0123456789.]", "");
							if(_temp3[1].Length > 0)
							{
								_pos[i].x = float.Parse( _temp3[1] );
								check = true;
							}
							break;
						case 2:
							_temp3[2] = Regex.Replace(_temp3[2], "[^0123456789.]", "");
							if(_temp3[2].Length > 0)
							{
								_pos[i].z = float.Parse( _temp3[2] );
								check = true;
							}
							break;
						default:
							break;
						}
					}
					if(check)
					{
						if(fast_point)
						{
							fast = _pos[i];
							fast_point = false;
						}

						if( max_x < _pos[i].x)	//ex : 比較最大值
						{
							max_x = _pos[i].x;
						}
						if( max_y < _pos[i].y)	//ex : 比較最大值
						{
							max_y = _pos[i].y;
						}

						i++;
					}
				}

				if(_temp1.Length >=3)
				{
					_pos[i] = fast;
					points_index_array[points_num-1] = i;

					if( max_x < _pos[i].x)	//ex : 比較最大值
					{
						max_x = _pos[i].x;
					}
					if( max_y < _pos[i].y)	//ex : 比較最大值
					{
						max_y = _pos[i].y;
					}

					i++;
					sw = 1;
				}
				else
				{
					sw = 3;
				}

				order++;

			}
			else
			{
				points_index_array[points_num-1] = i;
				sw = 999;
			}
			break;
		case 3:		//ex : 進階分析point
			if (max > order)
			{
				text = _File[order];

				string[] _temp1 = text.Split('"');
				string[] _temp2 = _temp1[0].Split(' ');
				
				for(int j=0 ; j < _temp2.Length ; j++)
				{
					string[] _temp3 = _temp2[j].Split(',');

					bool check = false;
					
					for(int k=0 ; k < _temp3.Length ; k++)
					{
						switch(k)
						{
						case 0:
							_temp3[0] = Regex.Replace(_temp3[0], "[^0123456789.]", "");
							if(_temp3[0].Length > 0)
							{
								_pos[i].y = float.Parse( _temp3[0] );
								check = true;
							}
							break;
						case 1:
							_temp3[1] = Regex.Replace(_temp3[1], "[^0123456789.]", "");
							if(_temp3[1].Length > 0)
							{
								_pos[i].x = float.Parse( _temp3[1] );
								check = true;
							}
							break;
						case 2:
							_temp3[2] = Regex.Replace(_temp3[2], "[^0123456789.]", "");
							if(_temp3[2].Length > 0)
							{
								_pos[i].z = float.Parse( _temp3[2] );
								check = true;
							}
							break;
						default:
							break;
						}
					}
					if(check)
					{
						if(fast_point)
						{
							fast = _pos[i];
							fast_point = false;
						}

						if( max_x < _pos[i].x)
						{
							max_x = _pos[i].x;
						}
						if( max_y < _pos[i].y)
						{
							max_y = _pos[i].y;
						}

						i++;
					}
				}
				
				if(_temp1.Length >=2)
				{
					_pos[i] = fast;

					points_index_array[points_num-1] = i;

					if( max_x < _pos[i].x)
					{
						max_x = _pos[i].x;
					}
					if( max_y < _pos[i].y)
					{
						max_y = _pos[i].y;
					}

					i++;
					sw = 1;
				}
				order++;
			}
			else
			{
				points_index_array[points_num-1] = i;
				sw = 999;
			}
			break;

		case 4:		//ex : 分析line
			if (max > order)
			{
				string[] _temp1 = text.Split('"');
				_temp1[1] = Regex.Replace(_temp1[1], "[^0123456789.-]", "");
				_temp1[3] = Regex.Replace(_temp1[3], "[^0123456789.-]", "");
				_temp1[5] = Regex.Replace(_temp1[5], "[^0123456789.-]", "");
				_temp1[7] = Regex.Replace(_temp1[7], "[^0123456789.-]", "");

				_pos[i].y = float.Parse(_temp1[1]);
				_pos[i].x = float.Parse(_temp1[3]);

				if( max_x < _pos[i].x)
				{
					max_x = _pos[i].x;
				}
				if( max_y < _pos[i].y)
				{
					max_y = _pos[i].y;
				}

				i++;
				_pos[i].y = float.Parse(_temp1[5]);
				_pos[i].x = float.Parse(_temp1[7]);

				line_index_array[line_num-1] = i;

				if( max_x < _pos[i].x)
				{
					max_x = _pos[i].x;
				}
				if( max_y < _pos[i].y)
				{
					max_y = _pos[i].y;
				}

				i++;
				order++;
				sw = 1;
			}
			else
			{
				line_index_array[line_num-1] = i;
				sw = 999;
			}
			break;

		case 5:		//ex : 分析path
			if (max > order)
			{
				for(int j = 0 ; j < text.Length ; j++)
				{
					switch(text[j])
					{
					case 'M':
						if(path_flag)
						{
							path_index_array[path_num-1] = i-1;
						}
						else
						{
							path_flag = true;
						}
						//
						path_start_array[path_num] = i;
						
						path_num ++;
						//
						j = str_num(j+1,1);					
						_pos[i] = num_vector[0];
						M_source = num_vector[0];

						if( max_x < _pos[i].x)
						{
							max_x = _pos[i].x;
						}
						if( max_y < _pos[i].y)
						{
							max_y = _pos[i].y;
						}

						i++;
						break;
					case 'L':
						j = str_num(j+1,1);
						_pos[i] = num_vector[0];

						if( max_x < _pos[i].x)
						{
							max_x = _pos[i].x;
						}
						if( max_y < _pos[i].y)
						{
							max_y = _pos[i].y;
						}

						i++;
						break;
					case 'l':
						j = str_num(j+1,1);
						_pos[i] = _pos[i-1] + num_vector[0];

						if( max_x < _pos[i].x)
						{
							max_x = _pos[i].x;
						}
						if( max_y < _pos[i].y)
						{
							max_y = _pos[i].y;
						}

						i++;
						break;
					case 'H':
						if(true)
						{
							int jj = 0;
							string _temp2 = "";

							for( jj = j+1 ; jj < text.Length ; jj++)
							{
								if( text[jj] == '-')
								{
									if(jj == (j+1))
									{
										_temp2 += text[jj];
									}
									else
									{
										break;
									}
								}
								else if( (text[jj] == '0')||(text[jj] == '1')||(text[jj] == '2')||(text[jj] == '3')||(text[jj] == '4')||(text[jj] == '5')||(text[jj] == '6')||(text[jj] == '7')||(text[jj] == '8')||(text[jj] == '9')||(text[jj] == '.') )
								{
									_temp2 += text[jj];
								}
								else
								{
									break;
								}
							}
							num_vector[0].y = float.Parse(_temp2);
							j = jj-1;
							num_vector[0].x = _pos[i-1].x;
							_pos[i] = num_vector[0];
							//Debug.Log(_pos[i] + "H");

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'h':
						if(true)
						{
							int jj = 0;
							string _temp2 = "";
							
							for( jj = j+1 ; jj < text.Length ; jj++)
							{
								if( text[jj] == '-')
								{
									if(jj == (j+1))
									{
										_temp2 += text[jj];
									}
									else
									{
										break;
									}
								}
								else if( (text[jj] == '0')||(text[jj] == '1')||(text[jj] == '2')||(text[jj] == '3')||(text[jj] == '4')||(text[jj] == '5')||(text[jj] == '6')||(text[jj] == '7')||(text[jj] == '8')||(text[jj] == '9')||(text[jj] == '.') )
								{
									_temp2 += text[jj];
								}
								else
								{
									break;
								}
							}
							num_vector[0].y = float.Parse(_temp2);
							j = jj-1;
							num_vector[0].x = 0;
							_pos[i] = _pos[i-1] + num_vector[0];
							//Debug.Log(_pos[i] + "H");

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'V':
						if(true)
						{
							int jj = 0;
							string _temp2 = "";
							
							for( jj = j+1 ; jj < text.Length ; jj++)
							{
								if( text[jj] == '-')
								{
									if(jj == (j+1))
									{
										_temp2 += text[jj];
									}
									else
									{
										break;
									}
								}
								else if( (text[jj] == '0')||(text[jj] == '1')||(text[jj] == '2')||(text[jj] == '3')||(text[jj] == '4')||(text[jj] == '5')||(text[jj] == '6')||(text[jj] == '7')||(text[jj] == '8')||(text[jj] == '9')||(text[jj] == '.') )
								{
									_temp2 += text[jj];
								}
								else
								{
									break;
								}
							}
							num_vector[0].x = float.Parse(_temp2);
							j = jj-1;
							num_vector[0].y = _pos[i-1].y;
							_pos[i] = num_vector[0];
						//	Debug.Log(_pos[i] + "V" + _temp2);

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'v':
						if(true)
						{
							int jj = 0;
							string _temp2 = "";
							
							for( jj = j+1 ; jj < text.Length ; jj++)
							{
								if( text[jj] == '-')
								{
									if(jj == (j+1))
									{
										_temp2 += text[jj];
									}
									else
									{
										break;
									}
								}
								else if( (text[jj] == '0')||(text[jj] == '1')||(text[jj] == '2')||(text[jj] == '3')||(text[jj] == '4')||(text[jj] == '5')||(text[jj] == '6')||(text[jj] == '7')||(text[jj] == '8')||(text[jj] == '9')||(text[jj] == '.') )
								{
									_temp2 += text[jj];
								}
								else
								{
									break;
								}
							}
						//	Debug.Log(j.ToString());
						//	Debug.Log(jj.ToString());
							num_vector[0].x = float.Parse(_temp2);
							j = jj-1;
						//	Debug.Log(text[j]);
							num_vector[0].y = 0;
							_pos[i] = _pos[i-1] + num_vector[0];
						//	Debug.Log(_pos[i] + "vv" + _temp2);

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'C':
						j = str_num(j+1,3);

						logic_V4[0] = _pos[i-1];
						logic_V4[1] = num_vector[0];
						logic_V4[2] = num_vector[1];
						logic_V4[3] = num_vector[2];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _c(logic_V4[0],logic_V4[1],logic_V4[2],logic_V4[3],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
					//	Debug.Log(_pos[i-1] + "C");
						break;
					case 'c':
						j = str_num(j+1,3);

						logic_V4[0] = _pos[i-1];
						logic_V4[1] = _pos[i-1] + num_vector[0];
						logic_V4[2] = logic_V4[0] + num_vector[1];
						logic_V4[3] = logic_V4[0] + num_vector[2];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _c(logic_V4[0],logic_V4[1],logic_V4[2],logic_V4[3],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
					//	Debug.Log(_pos[i-1] + "C");
						break;
					case 'S':
						j = str_num(j+1,2);
						
						logic_V4[0] = _pos[i-1];
						logic_V4[1] = (2 * logic_V4[3] ) - logic_V4[2];
						logic_V4[2] = num_vector[0];
						logic_V4[3] = num_vector[1];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _c(logic_V4[0],logic_V4[1],logic_V4[2],logic_V4[3],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 's':
						j = str_num(j+1,2);

						logic_V4[0] = _pos[i-1];
						logic_V4[1] = (2 * logic_V4[3] ) - logic_V4[2];
						logic_V4[2] = logic_V4[0] + num_vector[0];
						logic_V4[3] = logic_V4[0] + num_vector[1];



						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _c(logic_V4[0],logic_V4[1],logic_V4[2],logic_V4[3],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'Q':
						j = str_num(j+1,2);
						
						logic_V4[0] = _pos[i-1];
						logic_V4[1] = num_vector[0];;
						logic_V4[2] = num_vector[1];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _q(logic_V4[0],logic_V4[1],logic_V4[2],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'q':
						j = str_num(j+1,2);
						
						logic_V4[0] = _pos[i-1];
						logic_V4[1] = logic_V4[0] + num_vector[0];
						logic_V4[2] = logic_V4[0] + num_vector[1];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _q(logic_V4[0],logic_V4[1],logic_V4[2],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'T':
						j = str_num(j+1,2);
						
						logic_V4[0] = _pos[i-1];
						logic_V4[1] = (2 * logic_V4[2] ) - logic_V4[1];
						logic_V4[2] = num_vector[0];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _q(logic_V4[0],logic_V4[1],logic_V4[2],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 't':
						j = str_num(j+1,2);
						
						logic_V4[0] = _pos[i-1];
						logic_V4[1] = (2 * logic_V4[2] ) - logic_V4[1];
						logic_V4[2] = logic_V4[0] + num_vector[0];

						for(int k = 1 ; k <= t_Interval ; k++)
						{
							_pos[i] = _q(logic_V4[0],logic_V4[1],logic_V4[2],(k / t_Interval));

							if( max_x < _pos[i].x)
							{
								max_x = _pos[i].x;
							}
							if( max_y < _pos[i].y)
							{
								max_y = _pos[i].y;
							}

							i++;
						}
						break;
					case 'Z':
					case 'z':
						_pos[i] = M_source;

						if( max_x < _pos[i].x)
						{
							max_x = _pos[i].x;
						}
						if( max_y < _pos[i].y)
						{
							max_y = _pos[i].y;
						}

						i++;
						break;
					}
				}

				if(text.IndexOf("/>") != -1)
				{
					path_index_array[path_num-1] = i-1;
					sw = 1;
				}
				else
				{
					sw = 6;
				}
				order++;
			}
			else
			{
				sw = 999;
			}
			break;

		case 6:	//ex : 把path的資料流程重新跑一次
			if (max > order)
			{
				text = _File[order];
				sw = 5;
			}
			else
			{
				sw = 999;
			}
			break;


		case 999:	//ex : 讀檔結束
			sw = -1;
			if(i > 0)
			{
				Vector3 v3 = new Vector3(0,0,0);
				for(int j=0 ; j<i ; j++)
				{
					v3 = v3 + _pos[j];
				}
				_camera.V3 = new Vector3(v3.x/i,v3.z/i,v3.y/i);// v3/sum_point;
				_camera.Default_View = _camera.V3;
			}

			Array.Clear(action , 0 , action.Length );
			Array.Clear(work_order , 0 , work_order.Length );

			for(int n=0 ; n<2 ; n++)
			{
				for(int m = 0; m<1000;m++)
				{
					_GUI.List_order[n , m] = "";
					_GUI.List_type_num[n,m] = -1;
				}
			}

			_GUI.List_order_index[0] = 0;
			_GUI.List_order_index[1] = 0;
			for(int m = 0 ; m < points_num ; m++)
			{
				point_bool[m] = true;
			}

			for(int m = 0 ; m < line_num ; m++)
			{
				line_bool[m] = true;
			}

			for(int m = 0 ; m < path_num ; m++)
			{
				path_bool[m] = true;
			}
		//	Debug.Log("X = " + max_x.ToString() + " Y = " + max_y.ToString());
			//zaqxsw


			mix_sum = 0;
			mix_pos_array_index = 0;
			bool logic_2 = true;
			Rect mix_rect;
			mix_error += 0.005f;

			while(logic_2)
			{
				bool logic_1 = false;
				mix_start_array[mix_sum] = mix_pos_array_index;
				for(int v = 0 ; v < points_num ; v++)
				{
					if(point_bool[v])
					{
						logic_1 = true;
						point_bool[v] =false;
						for(int m = points_start_array[v] ; m <= points_index_array[v] ; m++)
						{
							mix_pos[mix_pos_array_index] = _pos[m];
							mix_pos_array_index++;
						}
						goto bbb;
					}
				}
				for(int v = 0 ; v < line_num ; v++)
				{
					if(line_bool[v])
					{
						logic_1 = true;
						line_bool[v] = false;
						for(int m = line_start_array[v] ; m <= line_index_array[v] ; m++)
						{
							mix_pos[mix_pos_array_index] = _pos[m];
							mix_pos_array_index++;
						}
						goto bbb;
					}
				}
				for(int v = 0 ; v < path_num ; v++)
				{
					if(path_bool[v])
					{
						logic_1 = true;
						path_bool[v] = false;
						for(int m = path_start_array[v] ; m <= path_index_array[v] ; m++)
						{
							mix_pos[mix_pos_array_index] = _pos[m];
							mix_pos_array_index++;
						}
						goto bbb;
					}
				}
			bbb:
				logic_2 = logic_1;
				if(logic_1)
				{
					for(int v = 0 ; v < points_num ; v++)
					{
						if(point_bool[v])
						{
							mix_rect = new Rect(mix_pos[mix_pos_array_index-1].x-mix_error,mix_pos[mix_pos_array_index-1].y-mix_error,2*mix_error,2*mix_error);

							//if(mix_pos[mix_pos_array_index-1] == _pos[points_start_array[v]])
							if(mix_rect.Contains(_pos[points_start_array[v]]))
							{
								point_bool[v] =false;
								for(int m = points_start_array[v] ; m <= points_index_array[v] ; m++)
								{
									mix_pos[mix_pos_array_index] = _pos[m];
									mix_pos_array_index++;
								}
								goto bbb;
							}
							//else if(mix_pos[mix_pos_array_index-1] == _pos[points_index_array[v]])
							else if(mix_rect.Contains(_pos[points_index_array[v]]))
							{
								point_bool[v] =false;
								for(int m = points_index_array[v] ; m >= points_start_array[v] ; m--)
								{
									mix_pos[mix_pos_array_index] = _pos[m];
									mix_pos_array_index++;
								}
								goto bbb;
							}
						}
					}
					for(int v = 0 ; v < line_num ; v++)
					{
						if(line_bool[v])
						{
							mix_rect = new Rect(mix_pos[mix_pos_array_index-1].x-mix_error,mix_pos[mix_pos_array_index-1].y-mix_error,2*mix_error,2*mix_error);

							//if(mix_pos[mix_pos_array_index-1] == _pos[line_start_array[v]])
							if(mix_rect.Contains(_pos[line_start_array[v]]))
							{
								line_bool[v] = false;
								for(int m = line_start_array[v] ; m <= line_index_array[v] ; m++)
								{
									mix_pos[mix_pos_array_index] = _pos[m];
									mix_pos_array_index++;
								}
								goto bbb;
							}
							//else if(mix_pos[mix_pos_array_index-1] == _pos[line_index_array[v]])
							else if(mix_rect.Contains(_pos[line_index_array[v]]))
							{
								line_bool[v] = false;
								for(int m = line_index_array[v] ; m >= line_start_array[v] ; m--)
								{
									mix_pos[mix_pos_array_index] = _pos[m];
									mix_pos_array_index++;
								}
								goto bbb;
							}
						}
					}
					for(int v = 0 ; v < path_num ; v++)
					{
						if(path_bool[v])
						{
							mix_rect = new Rect(mix_pos[mix_pos_array_index-1].x-mix_error,mix_pos[mix_pos_array_index-1].y-mix_error,2*mix_error,2*mix_error);

							//if(mix_pos[mix_pos_array_index-1] == _pos[path_start_array[v]])
							if(mix_rect.Contains(_pos[path_start_array[v]]))
							{
								path_bool[v] = false;
								for(int m = path_start_array[v] ; m <= path_index_array[v] ; m++)
								{
									mix_pos[mix_pos_array_index] = _pos[m];
									mix_pos_array_index++;
								}
								goto bbb;
							}
							//else if(mix_pos[mix_pos_array_index-1] == _pos[path_index_array[v]])
							else if(mix_rect.Contains(_pos[path_index_array[v]]))
							{
								path_bool[v] = false;
								for(int m = path_index_array[v] ; m >= path_start_array[v] ; m--)
								{
									mix_pos[mix_pos_array_index] = _pos[m];
									mix_pos_array_index++;
								}
								goto bbb;
							}
						}
					}
					mix_end_array[mix_sum] = mix_pos_array_index-1;
					mix_sum++;
				}
			}

			for(int m = 0 ; m < mix_sum ; m++)
			{
				_GUI.List_order[0 , _GUI.List_order_index[0]] = "Mix" + (m+1).ToString();
				_GUI.List_type_num[0,_GUI.List_order_index[0]] = m;
				_GUI.List_order_index[0] ++;
			}
			mix_error -= 0.005f;
//			for(int jj = 0 ; jj<mix_sum ; jj++)
//			{
//				Debug.Log(jj + " " + mix_pos[mix_start_array[jj]] + " " + mix_pos[mix_end_array[jj]]);
//			}

			_time = 0;
			break;
		default:
			break;
		}
	}



	
	void Save_gcode(string filename)	//ex : 將svg路徑存成gcode
	{
		max_x++;
		max_y++;

		bool Node_check = false;

		StreamWriter save_text = new StreamWriter(filename);

		save_text.WriteLine ("G21");
		save_text.WriteLine ("G90");
		save_text.WriteLine ("G92 X0 Y0 Z0");
	
		if(_GUI.sw_bool[2])
		{
			save_text.WriteLine ("M104 S290");
		}
		else if(_GUI.sw_bool[0])
		{
			save_text.WriteLine ("M83");	//E相對座標
		//	save_text.WriteLine ("G92 E0");
		}

		bool onetime_0 = true;

		float logic_x = 0, logic_y = 0, buf_x , buf_y;
		float old = 0 , temp;

		for(int m = 0 ; m<_GUI.List_order_index[1] ; m++)	//ex : Priority List的資料
		{
			if(_GUI.sw_bool[1])
			{
				save_text.WriteLine(Z_HIGH);
			}
			Node_check = true;

			for(int n=mix_start_array[_GUI.List_type_num[1,m]] ; n<=mix_end_array[_GUI.List_type_num[1,m]] ; n++)
			{
				buf_x = logic_x;
				buf_y = logic_y;
				logic_x =/* Math.Abs*/(mix_pos[n].x - Origin_Sphere.transform.position.x - (_GUI.x_reverse==true? max_x:0) )*Multiply_float;
				logic_y =/* Math.Abs*/(mix_pos[n].y - Origin_Sphere.transform.position.z - (_GUI.y_reverse==true? max_y:0) )*Multiply_float;

				if(_GUI.sw_bool[0])
				{
					if(onetime_0)
					{
						temp = angle_360 ( new Vector3(0,1,0) , new Vector3(logic_x , logic_y , 0) );
						old  = 0;
						onetime_0 = false;
					}
					else
					{
						temp = angle_360 ( new Vector3(0,1,0) , new Vector3(logic_x  - buf_x, logic_y - buf_y , 0) );
					}
					float buf_sub = (old - temp);
					if(Math.Abs(buf_sub) > 30)
					{
						save_text.WriteLine ("G1 Z" + Z_Height.ToString() + " F300");
						save_text.WriteLine ("G1 E" + buf_sub.ToString() + " F300");
						if(!Node_check)
						{
							save_text.WriteLine ("G1 Z0 F300");
						}
					}
					else
					{
						if(Node_check)
						{
							save_text.WriteLine ("G1 Z" + Z_Height.ToString() + " F300");
						}
						save_text.WriteLine ("G1 E" + buf_sub.ToString() + " F300");
					}
					old = temp;
				}

				save_text.WriteLine("G1 " + _GUI.xx + logic_x.ToString() + " " + _GUI.yy  + logic_y.ToString() + " F" + XY_FR );//+" E0");
				if(Node_check)
				{
					if(_GUI.sw_bool[1])
					{
						save_text.WriteLine(Z_LOW);
					}
					Node_check = false;

					if(_GUI.sw_bool[0])
					{
						save_text.WriteLine ("G1 Z0 F300");
					}
				}
			}
		}
		for(int m = 0 ; m<_GUI.List_order_index[0] ; m++)	//ex : Final List的資料
		{
			if(_GUI.sw_bool[1])
			{
				save_text.WriteLine(Z_HIGH);
			}
			Node_check = true;

			for(int n=mix_start_array[_GUI.List_type_num[0,m]] ; n<=mix_end_array[_GUI.List_type_num[0,m]] ; n++)
			{
				buf_x = logic_x;
				buf_y = logic_y;
				logic_x =/* Math.Abs*/(mix_pos[n].x - Origin_Sphere.transform.position.x - (_GUI.x_reverse==true? max_x:0) )*Multiply_float;
				logic_y =/* Math.Abs*/(mix_pos[n].y - Origin_Sphere.transform.position.z - (_GUI.y_reverse==true? max_y:0) )*Multiply_float;

				if(_GUI.sw_bool[0])
				{
					if(onetime_0)
					{
						temp = angle_360 ( new Vector3(0,1,0) , new Vector3(logic_x , logic_y , 0) );
						old  = 0;
						onetime_0 = false;
					}
					else
					{
						temp = angle_360 ( new Vector3(0,1,0) , new Vector3(logic_x  - buf_x, logic_y - buf_y , 0) );
					}
					float buf_sub = (old - temp);
					if(Math.Abs(buf_sub) > 30)
					{
						save_text.WriteLine ("G1 Z" + Z_Height.ToString() + " F300");
						save_text.WriteLine ("G1 E" + buf_sub.ToString() + " F300");
						if(!Node_check)
						{
							save_text.WriteLine ("G1 Z0 F300");
						}
					}
					else
					{
						if(Node_check)
						{
							save_text.WriteLine ("G1 Z" + Z_Height.ToString() + " F300");
						}
						save_text.WriteLine ("G1 E" + buf_sub.ToString() + " F300");
					}
					old = temp;
				}

				save_text.WriteLine("G1 " + _GUI.xx + logic_x.ToString() + " " + _GUI.yy + logic_y.ToString() + " F" + XY_FR );//+" E0");
				if(Node_check)
				{
					if(_GUI.sw_bool[1])
					{
						save_text.WriteLine(Z_LOW);
					}
					Node_check = false;

					if(_GUI.sw_bool[0])
					{
						save_text.WriteLine ("G1 Z0 F300");
					}
				}
			}
		}
		//save_text.WriteLine ("M440");
		if(_GUI.sw_bool[1])
		{
			save_text.WriteLine(Z_HIGH);
		}

		if(_GUI.sw_bool[2])
		{
			save_text.WriteLine ("G1 Z0.1 F1500");
			save_text.WriteLine ("G4 P100");
		}
		save_text.WriteLine ("M104 S0");
		save_text.WriteLine ("M84");
		//

		save_text.Close();
	}

	Vector3 _c(Vector3 cp0 , Vector3 cp1 , Vector3 cp2 , Vector3 cp3 , float t)	//ex : svg的計算邏輯
	{
		Vector3 result = new Vector3(0,0,0);

		float   ax, bx, cx;
		float   ay, by, cy;
		float   tSquared, tCubed;
		
		/*計算多項式係數*/
		
		cx = 3 * (cp1.x - cp0.x);
		bx = 3 * (cp2.x - cp1.x) - cx;
		ax = cp3.x - cp0.x - cx - bx;
		
		cy = 3 * (cp1.y - cp0.y);
		by = 3 * (cp2.y - cp1.y) - cy;
		ay = cp3.y - cp0.y - cy - by;
		
		/*計算位於參數值t的曲線點*/
		
		tSquared = t * t;
		tCubed = tSquared * t;
		
		result.x = (ax * tCubed) + (bx * tSquared) + (cx * t) + cp0.x;
		result.y = (ay * tCubed) + (by * tSquared) + (cy * t) + cp0.y;
//		result.x = (cp0.x * (1 - t) * (1 - t) * (1 - t)) + (3 * cp1.x * t * (1 - t) * (1 - t)) + (3 * cp2.x * t * t * (1 - t)) + (cp3.x * t * t * t);
//		result.y = (cp0.y * (1 - t) * (1 - t) * (1 - t)) + (3 * cp1.y * t * (1 - t) * (1 - t)) + (3 * cp2.y * t * t * (1 - t)) + (cp3.y * t * t * t);

		return result;
	}

	Vector3 _q(Vector3 cp0 , Vector3 cp1 , Vector3 cp2 , float t)	//ex : svg的計算邏輯
	{
		Vector3 result = new Vector3(0,0,0);

		float tSquared = t * t;

		result.y = ( tSquared * (cp0.x - (2*cp1.x) + cp2.x) ) + 2*t*(cp1.x - cp0.x) + cp0.x;
		result.x = ( tSquared * (cp0.y - (2*cp1.y) + cp2.y) ) + 2*t*(cp1.y - cp0.y) + cp0.y;

		return result;
	}

	private Vector3[] num_vector = new Vector3[3];
	int str_num(int index , int num)
	{
		int save_index = 0;
		int jj = 0;

		string _temp2 = "";
		int start_index = index;
		while(num > 0)
		{

			for( jj = start_index ; jj < text.Length ; jj++)
			{
				if( text[jj] == '-')
				{
					if(jj == start_index)
					{
						_temp2 += text[jj];
					}
					else
					{
						num_vector[save_index].y = float.Parse(_temp2);
						break;
					}
				}
				else if( (text[jj] == '0')||(text[jj] == '1')||(text[jj] == '2')||(text[jj] == '3')||(text[jj] == '4')||(text[jj] == '5')||(text[jj] == '6')||(text[jj] == '7')||(text[jj] == '8')||(text[jj] == '9')||(text[jj] == '.') )
				{
					_temp2 += text[jj];
				}
				else
				{
					jj++;
					num_vector[save_index].y = float.Parse(_temp2);
					break;
				}
				if(jj==(text.Length-1))
				{
					num_vector[save_index].y = float.Parse(_temp2);
					break;
				}
			}
			start_index = jj;
			_temp2 = "";

			for( jj = start_index ; jj < text.Length ; jj++)
			{
				if( text[jj] == '-')
				{
					if(jj == start_index)
					{
						_temp2 += text[jj];
					}
					else
					{
						num_vector[save_index].x = float.Parse(_temp2);
						break;
					}
				}
				else if( (text[jj] == '0')||(text[jj] == '1')||(text[jj] == '2')||(text[jj] == '3')||(text[jj] == '4')||(text[jj] == '5')||(text[jj] == '6')||(text[jj] == '7')||(text[jj] == '8')||(text[jj] == '9')||(text[jj] == '.') )
				{
					_temp2 += text[jj];
				}
				else
				{
					jj++;
					num_vector[save_index].x = float.Parse(_temp2);
					break;
				}
				if(jj==(text.Length-1))
				{
					num_vector[save_index].x = float.Parse(_temp2);
					break;
				}
			}
			start_index = jj;
			_temp2 = "";

			if(jj == text.Length)
			{
				break;
			}
			num--;
			save_index++;
		}
		return jj-2;
	}

	public Vector3 aaa = new Vector3(1,0,0) , bbb = new Vector3(0,0,0);
	float angle_360(Vector3 from_, Vector3 to_)	//ex : 計算角度
	{  
		Vector3 v3 = Vector3.Cross(from_,to_);  
		if(v3.z > 0)  
			return Vector3.Angle(from_,to_);  
		else  
			return 360-Vector3.Angle(from_,to_);  
	}
}

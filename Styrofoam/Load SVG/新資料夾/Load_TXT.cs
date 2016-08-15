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
	bool[] _show =new bool[1000000];

	//
	//float Standard = 5;
	//

	protected int i , order , max;

	string[] _File;


	private Material lineMaterial;

	static public int sw = -1;

	//player setting
	static public bool _CNC = false;
	static public int[,] action = new int[3,1000];
	static public int[,] work_order = new int[3,1000];
	//

	static public string XY_FR = "1500";
	static public string E_Ring_value = "360";
	static public string Z_Height = "200";
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

	const string Z_HIGH = "G1 Z0.1 F1500" + "\r\n" + "G4 P100" + "\r\n" + "M440" + "\r\n" + "G1 Z0 F1500" + "\r\n" + "G4 P1000" , Z_LOW = "G1 Z0.1 F1500" + "\r\n" + "G4 P100" + "\r\n" + "M441" + "\r\n" + "G1 Z0 F1500" + "\r\n" + "G4 P2000";

	static public float mix_error = 0;
	//G4 P200
//	const string Z_HIGH = "G1 Z200 F6000" , Z_LOW = "G1 Z0 F6000";

	void Start ()
	{
		points_num = 0;
		line_num = 0;
		path_num = 0;

		i = 0;

		sw = -1;

		if (!lineMaterial)
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
		if(Input.GetKeyDown(KeyCode.H))
		{
			show_hide = !show_hide;
		}

		if(sw == -1)
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

		if(_GUI.SaveFilePath.Length != 0)
		{

			Save_gcode(_GUI.SaveFilePath);
			_GUI.SaveFilePath = "";
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 120, 20),"FPS = " + ((int)(1/Time.deltaTime)).ToString());
		GUI.Label(new Rect(10, 30, 120, 20),"click ' H' on / off Hot Key List");
		if(show_hide)
		{
			GUI.Label(new Rect(10, 70, 400, 20),"Mouse Right Button + Mouse Move = Rotate view");
			GUI.Label(new Rect(10, 90, 400, 20),"Mouse Center Button + Mouse Move = Move view on XY plane");
			GUI.Label(new Rect(10, 110, 400, 20),"Shift + Center Mouse Button = Move Vertical View");
			GUI.Label(new Rect(10, 130, 400, 20),"Z = Default View");
			GUI.Label(new Rect(10, 150, 400, 20),"up, down or +, -  = single layer shift");
		}


		if((i > 0) && (sw == 0))
		{
			lineMaterial.SetPass( 0 );
			
			GL.Begin( GL.LINES );
			Color G_color = new Color(1,0,0);;
//			GL.Color (G_color);
//			for(int j=0;j<(i-1);j++)
//			{
//				GL.Vertex3(_pos[j].x , _pos[j].z , _pos[j].y);
//				GL.Vertex3(_pos[j+1].x , _pos[j+1].z , _pos[j+1].y);
//			}
			for(int m = 0 ; m < mix_sum ; m++)
			{
				for(int aa = 0 ; aa < 2 ; aa++)
				{
					for(int bb = 0 ; bb < _GUI.List_order_index[aa] ; bb++)
					{
						if(m == _GUI.List_type_num[aa,bb])
						{
							if(_GUI._id[aa,bb])
							{
								G_color = new Color(1,1,0);
							}
							else
							{
								G_color = new Color(1,0,0);
							}
						}
					}
				}
				GL.Color (G_color);
				for(int n = mix_start_array[m] ; n<mix_end_array[m] ; n++)
				{
					GL.Vertex3(mix_pos[n].x , mix_pos[n].z , mix_pos[n].y);
					GL.Vertex3(mix_pos[n+1].x , mix_pos[n+1].z , mix_pos[n+1].y);
				}
			}
			GL.End();
		}
		else
		{
			if(sw > 0)
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
	void logic(string filename)
	{
		switch (sw)
		{
		case 0:
			_File = File.ReadAllLines(filename);
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

				if(text.IndexOf("points=") != -1)
				{
					fast_point = true;
					sw = 2;
					text = text.Substring(text.IndexOf("points=")+1,text.Length -1 -text.IndexOf("points="));

					points_start_array[points_num] = i;

					points_num ++;
				}
				else if(text.IndexOf("<line") != -1)
				{
					sw = 4;
					text = text.Substring(text.IndexOf("x1")+1,text.Length -1 -text.IndexOf("x1"));
					line_start_array[line_num] = i;

					line_num ++;
				}
				else if(text.IndexOf("path") != -1)
				{
					sw = 5;
					text = text.Substring(text.IndexOf("d=")+1,text.Length -1 -text.IndexOf("d="));

					path_start_array[path_num] = i;

					path_num ++;
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
		case 2:
			if (max > order)
			{
				string[] _temp1 = text.Split('"');
				string[] _temp2 = _temp1[1].Split(' ');

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
								_pos[i].x = float.Parse( _temp3[0] );
								check = true;
							}
							break;
						case 1:
							_temp3[1] = Regex.Replace(_temp3[1], "[^0123456789.]", "");
							if(_temp3[1].Length > 0)
							{
								_pos[i].y = float.Parse( _temp3[1] );
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
							_show[i] = false;
						}
						else
						{
							_show[i] = true;
						}
						i++;
					}
				}

				if(_temp1.Length >=3)
				{
					_pos[i] = fast;
					_show[i] = true;
					points_index_array[points_num-1] = i;

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
		case 3:
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
								_pos[i].x = float.Parse( _temp3[0] );
								check = true;
							}
							break;
						case 1:
							_temp3[1] = Regex.Replace(_temp3[1], "[^0123456789.]", "");
							if(_temp3[1].Length > 0)
							{
								_pos[i].y = float.Parse( _temp3[1] );
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
							_show[i] = false;
						}
						else
						{
							_show[i] = true;
						}
						i++;
					}
				}
				
				if(_temp1.Length >=2)
				{
					_pos[i] = fast;
					_show[i] = true;

					points_index_array[points_num-1] = i;

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

		case 4:
			if (max > order)
			{
				string[] _temp1 = text.Split('"');
				_temp1[1] = Regex.Replace(_temp1[1], "[^0123456789.-]", "");
				_temp1[3] = Regex.Replace(_temp1[3], "[^0123456789.-]", "");
				_temp1[5] = Regex.Replace(_temp1[5], "[^0123456789.-]", "");
				_temp1[7] = Regex.Replace(_temp1[7], "[^0123456789.-]", "");

				_pos[i].x = float.Parse(_temp1[1]);
				_pos[i].y = float.Parse(_temp1[3]);
				_show[i] = false;
				i++;
				_pos[i].x = float.Parse(_temp1[5]);
				_pos[i].y = float.Parse(_temp1[7]);
				_show[i] = true;

				line_index_array[line_num-1] = i;

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

		case 5:
			if (max > order)
			{
				for(int j = 0 ; j < text.Length ; j++)
				{
					switch(text[j])
					{
					case 'M':
						j = str_num(j+1,1);					
						_pos[i] = num_vector[0];
						M_source = num_vector[0];
						_show[i] = false;
						i++;
						break;
					case 'L':
						j = str_num(j+1,1);
						_pos[i] = num_vector[0];
						_show[i] = true;
						i++;
						break;
					case 'l':
						j = str_num(j+1,1);
						_pos[i] = _pos[i-1] + num_vector[0];
						_show[i] = true;
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
							num_vector[0].x = float.Parse(_temp2);
							j = jj-1;
							num_vector[0].y = _pos[i-1].y;
							_pos[i] = num_vector[0];
							//Debug.Log(_pos[i] + "H");
							_show[i] = true;
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
							num_vector[0].x = float.Parse(_temp2);
							j = jj-1;
							num_vector[0].y = 0;
							_pos[i] = _pos[i-1] + num_vector[0];
							//Debug.Log(_pos[i] + "H");
							_show[i] = true;
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
							num_vector[0].y = float.Parse(_temp2);
							j = jj-1;
							num_vector[0].x = _pos[i-1].x;
							_pos[i] = num_vector[0];
						//	Debug.Log(_pos[i] + "V" + _temp2);
							_show[i] = true;
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
							num_vector[0].y = float.Parse(_temp2);
							j = jj-1;
						//	Debug.Log(text[j]);
							num_vector[0].x = 0;
							_pos[i] = _pos[i-1] + num_vector[0];
						//	Debug.Log(_pos[i] + "vv" + _temp2);
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
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
							_show[i] = true;
							i++;
						}
						break;
					case 'Z':
					case 'z':
						_pos[i] = M_source;
						_show[i] = true;
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

		case 6:
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


		case 999:
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

			break;
		default:
			break;
		}
	}
	void Save_gcode(string filename)
	{
		bool Node_check = false;

		StreamWriter save_text = new StreamWriter(filename);

		save_text.WriteLine ("G21");
		save_text.WriteLine ("G90");
		save_text.WriteLine ("G92 X0 Y0 Z0");
		save_text.WriteLine ("G1 Z0 F500");


		for(int m = 0 ; m<_GUI.List_order_index[1] ; m++)
		{
			save_text.WriteLine(Z_HIGH);
			Node_check = true;

			for(int n=mix_start_array[_GUI.List_type_num[1,m]] ; n<=mix_end_array[_GUI.List_type_num[1,m]] ; n++)
			{
				save_text.WriteLine("G1 X" + (mix_pos[n].x - Origin_Sphere.transform.position.x )*Multiply_float + " Y" + (mix_pos[n].y- Origin_Sphere.transform.position.z)*Multiply_float + " F" + XY_FR );//+" E0");
				if(Node_check)
				{
					save_text.WriteLine(Z_LOW);
					Node_check = false;
				}
			}
		}
		for(int m = 0 ; m<_GUI.List_order_index[0] ; m++)
		{
			save_text.WriteLine(Z_HIGH);
			Node_check = true;

			for(int n=mix_start_array[_GUI.List_type_num[0,m]] ; n<=mix_end_array[_GUI.List_type_num[0,m]] ; n++)
			{
				save_text.WriteLine("G1 X" + (mix_pos[n].x - Origin_Sphere.transform.position.x )*Multiply_float + " Y" + (mix_pos[n].y- Origin_Sphere.transform.position.z)*Multiply_float + " F" + XY_FR );//+" E0");
				if(Node_check)
				{
					save_text.WriteLine(Z_LOW);
					Node_check = false;
				}
			}
		}
		//save_text.WriteLine ("M440");
		save_text.WriteLine(Z_HIGH);

		save_text.WriteLine ("M104 S0");
	//	save_text.WriteLine ("G28 X0 Y0");
		save_text.WriteLine ("M84");
		//

		save_text.Close();
	}

	Vector3 _c(Vector3 cp0 , Vector3 cp1 , Vector3 cp2 , Vector3 cp3 , float t)
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

	Vector3 _q(Vector3 cp0 , Vector3 cp1 , Vector3 cp2 , float t)
	{
		Vector3 result = new Vector3(0,0,0);

		float tSquared = t * t;

		result.x = ( tSquared * (cp0.x - (2*cp1.x) + cp2.x) ) + 2*t*(cp1.x - cp0.x) + cp0.x;
		result.y = ( tSquared * (cp0.y - (2*cp1.y) + cp2.y) ) + 2*t*(cp1.y - cp0.y) + cp0.y;

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
	float angle_360(Vector3 from_, Vector3 to_)
	{  
		Vector3 v3 = Vector3.Cross(from_,to_);  
		if(v3.z > 0)  
			return Vector3.Angle(from_,to_);  
		else  
			return 360-Vector3.Angle(from_,to_);  
	}
}

﻿using UnityEngine;
using System.Collections;

//TODO Generate Sprite Based On User Selection
//TODO Special Combination Match
//TODO Special Combination Generate Different Level Sprites
//TODO If can be matched, Match at the user selected gem once, and then checkBoard

public class GenerateBoard : MonoBehaviour 
{
	//Sprites of creatures
	public Sprite[] lv1;
	public Sprite[] lv2;
	public Sprite[] lv3;
	public Sprite[] lv4;

	private Sprite[][] spriteList;//Sprites used in this game
	private GameObject[,] gems = new GameObject[7,7];//gems in game


	public GameObject gemPrefab;

	private bool matched = false;
	private bool enableUser = false;

	//Use for mouse interaction
	private GameObject selected;//The first selected gem
	private GameObject moveTo;//The second selected gem
	
	// Use this for initialization
	void Start () 
	{
		InitSpriteList();
		InitBoard();
		StartCoroutine(CheckBoard());
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(enableUser)
		{
			if(Input.GetMouseButtonDown(0))
			{
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Collider2D collider = Physics2D.OverlapPoint(mousePos);
				if(collider.gameObject.tag.Equals("gem") && selected != collider.gameObject)
				{
					if(selected != null)
					{

						int i1 = (int)(collider.transform.position.x + 0.1 - gemPrefab.transform.position.x);
						int j1 = (int)(collider.transform.position.y + 0.1 - gemPrefab.transform.position.y);
						int i2 = (int)(selected.transform.position.x + 0.1 - gemPrefab.transform.position.x);
						int j2 = (int)(selected.transform.position.y + 0.1 - gemPrefab.transform.position.y);
						selected.SendMessage("SetUnselected");//Clear the original selected
						selected = null;
						//Debug.Log("Swap from " + i2 + " " + j2 + " To " + i1 + j1);
						//Debug.Log(i1 + " " + j1);

						if((Mathf.Abs(i1-i2) == 1 && Mathf.Abs(j1-j2) == 0) || 
						   (Mathf.Abs(i1-i2) == 0 && Mathf.Abs(j1-j2) == 1))
						{
							if(CheckCanBeSwapped(i1,j1,i2,j2))
							{
								Debug.Log("Swapping");
								SwapMatchedGems(i1,j1,i2,j2);//Animation 0.5f time
								StartCoroutine(CheckBoard());//Wait 0.6f time at start
							}
							else
							{
								StartCoroutine(SwapUnmatchedGems(i1,j1,i2,j2));
							}
						}
					}
					else
					{
						selected = collider.gameObject;
						selected.SendMessage("SetSelected");
					}


				}
			}
		}
	}

	void InitSpriteList()
	{
		//Select 4 sprites from each alignment(6 alignments in total)
		//TODO generate sprites based on player selection
		spriteList = new Sprite[][]
		{new Sprite[]{lv1[1],lv1[3],lv1[5],lv1[7],lv1[9],lv1[11]},
			new Sprite[]{lv2[1],lv2[3],lv2[5],lv2[7],lv2[9],lv2[11]},
			new Sprite[]{lv3[1],lv3[3],lv3[5],lv3[7],lv3[9],lv3[11]},
			new Sprite[]{lv4[1],lv4[3],lv4[5],lv4[7],lv4[9],lv4[11]}};
	}

	void InitBoard()
	{		
		Debug.Log ("Init board");
		for(int i=0;i<gems.GetLength(0);i++)
		{
			for(int j=0;j<gems.GetLength(1);j++)
			{
				gems[i,j] = SpawnNewLv1Cube(i,j);
			}
		}
	}

	GameObject SpawnNewLv1Cube(int i, int j)
	{
		int temp=Random.Range(0,5);
		GameObject cube= Instantiate(gemPrefab,
		                             new Vector3(gemPrefab.transform.position.x + i,
		            							 gemPrefab.transform.position.y + j,0),
		                            			 Quaternion.identity)as GameObject;
		
		SpriteRenderer sr = cube.renderer as SpriteRenderer;
		sr.sprite = spriteList[0][temp];

		iTween.Init(cube);
		iTween.RotateBy(cube, new Vector3(0,0,-8), 1.5f);
		iTween.ScaleFrom(cube, new Vector3(0,0,0), 2f);

		cube.name = "1 " + temp;
		cube.SendMessage("SetX", i);
		cube.SendMessage("SetY", j);

		return cube;
	}

	IEnumerator CheckBoard() 
	{
		matched = false;
		yield return new WaitForSeconds(0.6f);  //Wait for animation to finish
		int sames = 0;
		//Detect vertical matches
		for(int i = gems.GetLength(0)-1; i >=0 ; i--)
		{
			for(int j = gems.GetLength(1)-1; j >= 2; j--)
			{
				sames = 0;
				if(CheckSameColor(gems[i,j],gems[i,j-1]))
				{
					sames++;
				}
				else
				{
					continue;
				}
				if(CheckSameColor(gems[i,j-1],gems[i,j-2]))//sames == 2, match 3
				{
					sames++;
				}
				else
				{
					continue;
				}
				
				if(j >= 3)
				{
					if(CheckSameColor(gems[i,j-2],gems[i,j-3]))//sames == 3, match 4
					{
						sames++;
						if(j >= 4)
						{
							if(CheckSameColor(gems[i,j-3],gems[i,j-4]))//sames == 4, match 5
							{
								sames++;
							}
						}
					}
				}
				//If 3 or more same cubes 
				//match them and make them disappear and spawn/fall new cubes
				
				if (sames >= 2)
				{
					//Destory cubes
					for(int n = 0; n <= sames; n++)
					{
						iTween.RotateBy(gems[i,j-n], new Vector3(0,0,8), 1f);
						iTween.ScaleTo(gems[i,j-n], new Vector3(0,0,0), 1.5f);
						//The actual gem game object is collected below after animation finished
					}

					yield return new WaitForSeconds(0.2f); 


					for(int n = 0; n <= sames; n++)
					{
						Destroy(gems[i,j-n]);
					}

					//Fall/Spawn new cubes
					int c = j-sames;
					
					while(c <= 6)
					{
						if(c+sames+1 <= 6)
						{
							gems[i,c] = gems[i,c+sames+1];					
						}
						else
						{
							gems[i,c] = SpawnNewLv1Cube(i,9);						
						}
						iTween.MoveTo(gems[i,c],new Vector3(gemPrefab.transform.position.x + i, 
						                                    gemPrefab.transform.position.y + c,0),
						              2f);
						gems[i,c].SendMessage("SetX", i);
						gems[i,c].SendMessage("SetY", c);
						yield return new WaitForSeconds(0.05f); 
						c++;
					}	
					matched = true;
				}
			}
		}
		
		//Detect Horizontal Match
		sames = 0;
		
		for(int j = gems.GetLength(1)-1; j >=0 ; j--)
		{
			for(int i = 0; i < gems.GetLength(0)-2; i++)
			{
				sames = 0;
				if(CheckSameColor(gems[i,j],gems[i+1,j]))
				{
					sames++;
				}
				else
				{
					continue;
				}
				if(CheckSameColor(gems[i+1,j],gems[i+2,j]))//sames == 2, match 3
				{
					sames++;
				}
				else
				{
					continue;
				}
				
				if(i <= 3)
				{
					if(CheckSameColor(gems[i+2,j],gems[i+3,j]))//sames == 3, match 4
					{
						sames++;
						if(i <= 2)
						{
							if(CheckSameColor(gems[i+3,j],gems[i+4,j]))//sames == 4, match 5
							{
								sames++;
							}
						}
					}

				}
				

				
				//If 3 or more same cubes 
				//match them and make them disappear and spawn/fall new cubes
				
				if (sames >= 2)
				{
					//Destory cubes
					for(int n = 0; n <= sames; n++)
					{;
						iTween.RotateBy(gems[i+n,j], new Vector3(0,0,8), 1f);
						iTween.ScaleTo(gems[i+n,j], new Vector3(0,0,0), 1.5f);
					}
					yield return new WaitForSeconds(0.2f); 


					for(int n = 0; n <= sames; n++)
					{
						Destroy(gems[i+n,j]);
					}

					for(int w = 0; w <= sames; w++)
					{
						for(int h = j; h <= gems.GetLength(1)-1; h++)
						{
							
							//Fall/Spawn new cubes
							if(h < gems.GetLength(1)-1)
							{
								gems[i+w,h] = gems[i+w,h+1];
							}
							else
							{
								gems[i+w,h] = SpawnNewLv1Cube(i+w,9);
							}
							
							iTween.MoveTo(gems[i+w,h], new Vector3(gemPrefab.transform.position.x + i + w,
							                                       gemPrefab.transform.position.y + h,0),
							              2f);
							gems[i+w,h].SendMessage("SetX", i+w);
							gems[i+w,h].SendMessage("SetY", h);
							
						}
						yield return new WaitForSeconds(0.05f); 
					}
					matched = true;				
				}
			}
		}
		
		if(matched)
		{
			StartCoroutine(CheckBoard());
		}
		else
		{
			enableUser = true;
		}
	} 

	//Take 2 cubes as arguments
	bool CheckSameColor(GameObject gb1, GameObject gb2)
	{
		if(!gb1 || !gb2)
		{
			return false;
		}
		return (gb1.name.Substring(2).Equals(gb2.name.Substring(2))) ;
	}

	bool CheckSameColor(int i1, int j1, int i2, int j2)
	{
		if(i1 >= gems.GetLength(0) || i2 >= gems.GetLength(0) || j1 >= gems.GetLength(1) || j2 >= gems.GetLength(1) ||
		   i1 < 0 || i2 <0 || j1 < 0 || j2 <0 )
		{
			return false;
		}
		else
		{
			Debug.Log(i1 + " " + j1 + " " + i2 + " " + j2);
			return CheckSameColor(gems[i1,j1], gems[i2,j2]);
		}
	}

	bool checkMatchExist(string[,] gems)
	{
		int sames = 0;
		//Detect vertical matches
		for(int i = gems.GetLength(0)-1; i >=0 ; i--)
		{
			for(int j = gems.GetLength(1)-1; j >= 2; j--)
			{
				sames = 0;
				if(gems[i,j].Equals(gems[i,j-1]))
				{
					sames++;
				}
				else
				{
					continue;
				}
				if((gems[i,j-1].Equals(gems[i,j-2])))//sames == 2, match 3
				{
					sames++;
				}
				else
				{
					continue;
				}

				if(sames >= 2)
				{
					return true;
				}
			}
		}

		//Detect Horizontal Match
		sames = 0;		
		for(int j = gems.GetLength(1)-1; j >=0 ; j--)
		{
			for(int i = 0; i < gems.GetLength(0)-2; i++)
			{
				sames = 0;
				if(gems[i,j].Equals(gems[i+1,j]))
				{
					sames++;
				}
				else
				{
					continue;
				}
				if(gems[i+1,j].Equals(gems[i+2,j]))//sames == 2, match 3
				{
					sames++;
				}
				else
				{
					continue;
				}
				
				if (sames >= 2)
				{
					return true;
				}
			}
		}

		return false;
	}

	bool CheckCanBeSwapped(int i1, int j1, int i2, int j2)
	{
		if( (Mathf.Abs(i1-i2) == 1 && Mathf.Abs(j1-j2) == 0) || 
		    (Mathf.Abs(i1-i2) == 0 && Mathf.Abs(j1-j2) == 1))
		{
			string[,] temp = new string[gems.GetLength(0),gems.GetLength(1)];
			for(int i=0;i<gems.GetLength(0);i++)
			{
				for(int j=0;j<gems.GetLength(1);j++)
				{
					temp[i,j] = gems[i,j].name;
				}
			}
			string gemTemp = temp[i1,j1].Substring(0);
			temp[i1,j1] = temp[i2,j2].Substring(0);
			temp[i2,j2] = gemTemp;
			return checkMatchExist(temp);;
		}
		return false;
	}

	void SwapMatchedGems(int i1, int j1, int i2, int j2)
	{
		enableUser = false; //EnableUser will be true after CheckBoard()
		iTween.MoveTo(gems[i1,j1], gems[i2,j2].transform.position, 0.5f);
		iTween.MoveTo(gems[i2,j2], gems[i1,j1].transform.position, 0.5f);
		GameObject temp = gems[i1,j1];
		gems[i1,j1] = gems[i2,j2];
		gems[i2,j2] = temp;
	}

	IEnumerator SwapUnmatchedGems(int i1, int j1, int i2, int j2)
	{
		enableUser = false; //EnableUser will be true after CheckBoard()
		iTween.MoveTo(gems[i1,j1], gems[i2,j2].transform.position, 0.5f);
		iTween.MoveTo(gems[i2,j2], gems[i1,j1].transform.position, 0.5f);
		yield return new WaitForSeconds(0.5f);
		iTween.MoveTo(gems[i2,j2], gems[i1,j1].transform.position, 0.5f);
		iTween.MoveTo(gems[i1,j1], gems[i2,j2].transform.position, 0.5f);
		yield return new WaitForSeconds(0.5f);
		enableUser = true;
	}
}




using UnityEngine;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public class BrickBreaker : MonoBehaviour {
 const int Rows=15, Cols=24, MaxBalls=100;
 const float R=.065f, DX=.32f, DY=.23f, BW=.29f, BH=.20f;
 class Ball { public Vector2 p,v; public GameObject g; }
 List<Ball> balls=new List<Ball>();
 GameObject[,] tiles=new GameObject[Rows,Cols];
 GameObject board,bat; Sprite square,circle; Camera cam;
 int score,lives,left; float paddle; bool launched,ended;
 string bonus=""; float bonusUntil;
 Sprite MakeSprite(bool round){ var t=new Texture2D(64,64); for(int y=0;y<64;y++)for(int x=0;x<64;x++)t.SetPixel(x,y,!round||(new Vector2(x-31.5f,y-31.5f)).sqrMagnitude<992?Color.white:Color.clear); t.Apply(); return Sprite.Create(t,new Rect(0,0,64,64),Vector2.one*.5f,64); }
 GameObject Shape(string n,Vector2 p,Vector2 size,Color color,bool round=false){var g=new GameObject(n);g.transform.SetParent(board.transform);g.transform.position=p;g.transform.localScale=new Vector3(size.x,size.y,1);var s=g.AddComponent<SpriteRenderer>();s.sprite=round?circle:square;s.color=color;s.sortingOrder=round?2:0;return g;}
 Vector2 Pos(int r,int c){return new Vector2((c-(Cols-1)*.5f)*DX,3.9f-r*DY);}
 void Start(){square=MakeSprite(false);circle=MakeSprite(true);cam=Camera.main;if(!cam){var g=new GameObject("Main Camera");g.tag="MainCamera";cam=g.AddComponent<Camera>();}cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.045f,.065f,.11f);ResetGame();}
 void AddBall(Vector2 p,Vector2 v){if(balls.Count>=MaxBalls)return;balls.Add(new Ball{p=p,v=v,g=Shape("Ball",p,Vector2.one*R*2,Color.white,true)});}
 void ResetGame(){if(board){board.SetActive(false);Destroy(board);}balls.Clear();board=new GameObject("Multiball Board");score=0;lives=3;left=Rows*Cols;paddle=0;ended=false;bonus="";bonusUntil=0;for(int r=0;r<Rows;r++)for(int c=0;c<Cols;c++)tiles[r,c]=Shape("Brick",Pos(r,c),new Vector2(BW,BH),Color.HSVToRGB(r*.72f/(Rows-1),.72f,1));Shape("Left wall",new Vector2(-4.04f,0),new Vector2(.08f,9.5f),Color.gray);Shape("Right wall",new Vector2(4.04f,0),new Vector2(.08f,9.5f),Color.gray);Shape("Top wall",new Vector2(0,4.75f),new Vector2(8,.08f),Color.gray);bat=Shape("Paddle",new Vector2(0,-4),new Vector2(1.6f,.2f),Color.cyan);Ready();}
 void Ready(){launched=false;AddBall(new Vector2(paddle,-3.82f),Vector2.zero);}
 void Update(){cam.orthographicSize=Mathf.Max(5.6f,4.4f/Mathf.Max(.01f,cam.aspect));float move=0;bool fire=false,restart=false;
#if ENABLE_INPUT_SYSTEM
 var k=Keyboard.current;if(k!=null){move=(k.rightArrowKey.isPressed||k.dKey.isPressed?1:0)-(k.leftArrowKey.isPressed||k.aKey.isPressed?1:0);fire=k.spaceKey.wasPressedThisFrame;restart=k.rKey.wasPressedThisFrame;}
#elif ENABLE_LEGACY_INPUT_MANAGER
 move=(Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A)?1:0);fire=Input.GetKeyDown(KeyCode.Space);restart=Input.GetKeyDown(KeyCode.R);
#endif
 if(restart)ResetGame();if(ended)return;paddle=Mathf.Clamp(paddle+move*8*Time.deltaTime,-3.2f,3.2f);bat.transform.position=new Vector2(paddle,-4);if(!launched){balls[0].p=new Vector2(paddle,-3.82f);if(fire){launched=true;balls[0].v=new Vector2(2.5f,6.5f);}}
 if(launched){float dt=Mathf.Min(Time.deltaTime,.05f);int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.003f));for(int s=0;s<steps&&!ended&&launched;s++){int count=balls.Count;for(int i=count-1;i>=0&&!ended;i--){Ball b=balls[i];Step(b,dt/steps);if(b.p.y< -5){Destroy(b.g);balls.RemoveAt(i);}}if(balls.Count==0&&!ended){lives--;if(lives==0)ended=true;else Ready();}}}foreach(var b in balls)b.g.transform.position=b.p;}
 void Step(Ball b,float dt){Vector2 old=b.p;b.p+=b.v*dt;if(b.p.x< -4+R||b.p.x>4-R){b.p.x=Mathf.Clamp(b.p.x,-4+R,4-R);b.v.x=-b.v.x;}if(b.p.y>4.71f-R){b.p.y=4.71f-R;b.v.y=-Mathf.Abs(b.v.y);}if(b.v.y<0&&old.y>=-3.9f+R&&b.p.y<=-3.9f+R&&Mathf.Abs(b.p.x-paddle)<=.8f+R){b.p.y=-3.9f+R;float x=Mathf.Clamp((b.p.x-paddle)/.8f,-1,1);b.v=new Vector2(x*5,5).normalized*Mathf.Min(10,b.v.magnitude+.06f);}
 int cc=Mathf.RoundToInt(b.p.x/DX+(Cols-1)*.5f),rr=Mathf.RoundToInt((3.9f-b.p.y)/DY);for(int r=Mathf.Max(0,rr-1);r<=Mathf.Min(Rows-1,rr+1);r++)for(int c=Mathf.Max(0,cc-1);c<=Mathf.Min(Cols-1,cc+1);c++){if(!tiles[r,c])continue;Vector2 p=Pos(r,c);float hx=BW*.5f+R,hy=BH*.5f+R;if(Mathf.Abs(b.p.x-p.x)>hx||Mathf.Abs(b.p.y-p.y)>hy)continue;float ox=hx-Mathf.Abs(b.p.x-p.x),oy=hy-Mathf.Abs(b.p.y-p.y);if(Mathf.Abs(old.y-p.y)>=hy||(Mathf.Abs(old.x-p.x)<hx&&oy<=ox)){float sign=old.y>=p.y?1:-1;b.p.y=p.y+sign*(hy+.001f);b.v.y=sign*Mathf.Abs(b.v.y);}else{float sign=old.x>=p.x?1:-1;b.p.x=p.x+sign*(hx+.001f);b.v.x=sign*Mathf.Abs(b.v.x);}tiles[r,c].SetActive(false);Destroy(tiles[r,c]);tiles[r,c]=null;score+=10;left--;if(left==0){ended=true;return;}float chance=Random.value;int extra=chance<.05f?3:chance<.10f?2:chance<.25f?1:0;int added=Mathf.Min(extra,MaxBalls-balls.Count);for(int j=0;j<added;j++){float a=Random.Range(-65f,65f)*Mathf.Deg2Rad;Vector2 v=new Vector2(Mathf.Sin(a),Mathf.Cos(a)* (b.v.y>=0?1:-1))*7;AddBall(b.p,v);}if(added>0){bonus="+"+added+" BALLS!";bonusUntil=Time.time+1.1f;}return;}}
 void OnGUI(){var s=new GUIStyle(GUI.skin.label);s.fontSize=Mathf.Clamp(Screen.width/45,13,24);s.normal.textColor=Color.white;GUI.Label(new Rect(12,8,Screen.width/3,35),"SCORE: "+score,s);s.alignment=TextAnchor.UpperCenter;GUI.Label(new Rect(Screen.width/3,8,Screen.width/3,35),"BALLS: "+balls.Count,s);s.alignment=TextAnchor.UpperRight;GUI.Label(new Rect(Screen.width*2/3-12,8,Screen.width/3,35),"LIVES: "+lives,s);s.alignment=TextAnchor.MiddleCenter;GUI.Label(new Rect(0,Screen.height-35,Screen.width,30),"A / D or Arrows | SPACE: Launch | R: Restart",s);if(Time.time<bonusUntil&&!ended){s.normal.textColor=Color.cyan;GUI.Label(new Rect(0,Screen.height*.68f,Screen.width,35),bonus,s);}if(ended||!launched){s.fontSize=30;s.normal.textColor=Color.white;GUI.Label(new Rect(0,Screen.height*.53f,Screen.width,70),ended?(left==0?"YOU WIN!":"GAME OVER")+" - R to restart":"Press SPACE",s);}}
}

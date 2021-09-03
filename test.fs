#version 300 es
precision mediump float;
in vec3 camPos;
in vec2 screenSize;

in vec3 input1;

out vec4 color;



//float mod(float a,float b) {
//  return a - (b * floor(a/b));
//}

float Dist(vec3 pos1,vec3 pos2) {
  return sqrt( (pos1.x-pos2.x)*(pos1.x-pos2.x) + (pos1.y-pos2.y)*(pos1.y-pos2.y) + (pos1.z-pos2.z)*(pos1.z-pos2.z) );
}

float MaxDist(vec3 pos1,vec3 pos2) {
  return max(max(abs(pos1.x-pos2.x),abs(pos1.y-pos2.y)),abs(pos1.z-pos2.z));
}

/*
float MaxDist(vec3 pos1, vec3 pos2) {
  return max( max( (pos1.x-pos2.x),(pos1.y-pos2.y) ) ,(pos1.z-pos2.z) );
}

float SphereDist(vec3 spherePos,vec3 rayPos) {
  //vec3 dist = spherePos - rayPos;
  //vec3 dist = rayPos - spherePos;
  return Dist(spherePos,rayPos)-10.0;
}

float SphereModDist(vec3 spherePos,vec3 rayPos,float r) {
  return Dist(spherePos,mod(rayPos,50.0)-r*2.0)-r;
  //return norm(mod(rayPos,50.0)-spherePos)-r;
}
*/

struct RayData {
  float MinDist;
  int Calls;
  bool HitObject;
  vec3 Pos;
  vec3 Dir;
  int Index;
  vec4 Color;
};


struct DistanceObject {
  vec3 Pos;
  float Radius;
  vec4 Color;
  bool null;
  int Index;
  int DistanceFunction;

  bool Reflective;

  //float Dist(vec3 objPos,vec3 rayPos,float r) {
  //  return Dist(objPos,rayPos)-r;
  //}
};



//Distance equations, the core of the rendering engine.

#define MAX_OBJS 2

#define DE_SPHERE 0
#define DE_INF_SPHERE 1
#define DE_CUBE 2
#define DE_INF_CUBE 3


DistanceObject nullobj = DistanceObject(vec3(0,0,0),0.0,vec4(0,0,0,1),true,-1,DE_SPHERE,false);

float SphereDist(vec3 objPos,vec3 rayPos,float r) {
  return Dist(objPos,rayPos)-r;

  //return Dist(objPos,mod(rayPos,50.0)-25.0)-r;

  //return MaxDist(objPos,mod(rayPos,50.0)-25.0)-r;
}

float ModSphereDist(vec3 objPos,vec3 rayPos,float r) {
  return Dist(objPos,mod(rayPos,50.0)-25.0)-r;
}

float CubeDist(vec3 objPos,vec3 rayPos,float r) {
  return MaxDist(objPos,rayPos)-r;
}

float ModCubeDist(vec3 objPos,vec3 rayPos,float r) {
  return MaxDist(objPos,mod(rayPos,50.0)-25.0)-r;
}




float GetDist(DistanceObject obj,int de,vec3 pos) {
  switch(de) {
    case DE_SPHERE:
      return SphereDist(obj.Pos,pos,obj.Radius);
      break;

    case DE_INF_SPHERE:
      return ModSphereDist(obj.Pos,pos,obj.Radius);
      break;

    case DE_CUBE:
      return CubeDist(obj.Pos,pos,obj.Radius);
      break;
    case DE_INF_CUBE:
      return ModCubeDist(obj.Pos,pos,obj.Radius);
      break;
  }

  return -1.0;
}



RayData MarchRay(vec3 pos2,vec3 dir2,DistanceObject obj[MAX_OBJS]) {


  vec3 dir = normalize(dir2);
  vec3 pos = pos2;

  float dist = 999.0;

  float mindist = 999.0;

  float limit = 0.1;


  int maxCalls = 100;//30

  int calls = 0;

  vec4 colorOut = vec4(0,0,0,1);

  int id = -1;

  for (calls = 0;calls<maxCalls; calls++) {
    dist = 99999.0;

    for (int objs = 0;objs<MAX_OBJS;objs++) {
      if (obj[objs].null) {continue;}
      float inDist;

      inDist = GetDist(obj[objs],obj[objs].DistanceFunction,pos);

      if (inDist < dist) {
        dist = inDist;
        id = objs;
        colorOut = obj[objs].Color;
      }

    }


    pos += dir*dist;


    if (dist<=limit&&!obj[id].Reflective) {break;}

    if (dist<=limit) {pos-=dir*0.0;dir = 1.0 - dir;}
    //if (dist<=limit) {break;}
    mindist = min(mindist,dist);
    //break;
  }

  if (dist<=limit) {return RayData(mindist,calls,true,pos,dir,id,colorOut);}
  return RayData(mindist,calls,false,pos,dir,id,colorOut);





}



vec3[2] Perspective(vec2 pixelSpace,vec3 camIn,vec2 fov) {
  vec2 grid2d = pixelSpace*fov-fov*0.5;
  vec3 grid = vec3(1.0,grid2d.yx);


    vec3 rayRot = grid;
    return vec3[2]( vec3(camIn),vec3(rayRot) );
}

/*
vec3[2] Perspective2(vec2 pixelSpace,vec3 camIn) {
vec2 grid2d = pixelSpace*2.0-1.0;
vec3 grid = vec3(1.0,grid2d.yx);


  vec3 rayRot = normalize(grid);
  return vec3[2]( vec3(camIn.x,camIn.yz+grid2d.yx*2.0),vec3(rayRot) );
}*/


vec3[2] PerspectiveInvert(vec2 pixelSpace,vec3 camIn) {
  float fov = 50.0;

  vec2 grid2d = pixelSpace*fov-fov/2.0;
  vec3 grid = vec3(50.0+camIn.x,camIn.yz);


    vec3 rayRot = grid - vec3(camIn.x,camIn.yz-grid2d.yx);
    return vec3[2]( vec3(camIn.x,camIn.yz-grid2d.yx),vec3(rayRot) );
}

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

vec3[2] Zoom(vec2 pixelSpace,vec3 camIn) {
  vec2 grid2d = pixelSpace*2.0-1.0;

  vec3 grid = vec3(1.0,1.0/grid2d.yx);


    vec3 rayRot = grid;
    return vec3[2]( vec3(camIn),vec3(rayRot) );
}


vec3[2] ptest(vec2 pixelSpace,vec3 camIn) {
  vec2 grid2d = pixelSpace*2.0-1.0;
  vec3 grid = vec3(1.0,normalize(grid2d.yx));


    vec3 rayRot = grid;
    return vec3[2]( vec3(camIn),vec3(rayRot) );
}



vec3 BigPerspective(vec2 pixelSpace) {
  vec2 grid2d = pixelSpace*2.0-1.0;
  return vec3(1.0,grid2d.xy*5.0);
}

vec3 SortofOrtho(vec2 pixelSpace) {
  vec2 grid2d = pixelSpace*2.0-1.0;

  return vec3(0.0,grid2d.yx);
}




vec3[2] Ortho(vec2 pixelSpace,vec3 camIn) {
  vec2 grid2d = pixelSpace*50.0-25.0;

  vec3 grid = vec3(1.0,0.0,0.0);
  return vec3[2]( vec3(camIn.x,camIn.zy+grid2d.xy),grid );
}


vec3[2] Sphere(vec2 pixelSpace,vec3 camIn) {
  vec2 grid2d = pixelSpace*2.0-1.0;
  vec3 grid = vec3(2.0,grid2d.yx);


    vec3 rayRot = grid;
    return vec3[2]( vec3(camIn),vec3(rayRot) );
}


void main() {
  vec3 lightPos = vec3(-50,50,50);

  DistanceObject lightObj = DistanceObject(lightPos,5.0,vec4(1,1,1,1),false,1,DE_SPHERE,false);

  vec3 objPos = vec3(0,0,0);

  //vec3 camPos = vec3(0,0,0);
  vec3 camRot = vec3(1,0,0);

  vec2 pixelSpace = gl_FragCoord.xy/screenSize;




  vec3 OUT[2] = Perspective(pixelSpace,camPos,vec2(1,1));

  vec3 rayRot = OUT[1];
  vec3 camPos1 = OUT[0];





  //if (MarchRay(camPos1,rayRot,DistanceObject[2](  DistanceObject(input1,5.0,vec4(1,0,0,1),false,0,DE_INF_CUBE,false ), DistanceObject(lightPos,5.0,vec4(1,1,1,1),false,1,DE_SPHERE,false )  )).HitObject) {color = vec4(1,0,0,1);} else {color = vec4(0,0,0,1);}
  //return;

  RayData data = MarchRay(camPos1,rayRot,DistanceObject[2](  DistanceObject(objPos,5.0,vec4(1,0,0,1),false,0,DE_INF_CUBE,false ), DistanceObject(lightPos,5.0,vec4(1,1,1,1),false,1,DE_SPHERE,false )  ));

  if (data.HitObject) {

    //calc lighting:

    //vec3 rayPos = data.Pos-data.Dir*1.0;

    rayRot = lightPos - data.Pos;

    vec3 rayPos = data.Pos+rayRot*0.1;//0.5, or 1.0, or maybe 0.1

    RayData lightData = MarchRay(rayPos,rayRot,DistanceObject[2](  DistanceObject(objPos,5.0,vec4(1,0,0,1),false,0,DE_INF_CUBE,false ),lightObj  ));

    float callAmount = 100.0;

    float lightAmount = 25.0;//25
    float lightFadeAmount = 1.0;//0.8

    //float amount = (callAmount-float(lightData.Calls))/callAmount;
    //float amount = min(max(lightData.MinDist,0.1),1.0);

    //or distance based:

    //get distance to light source
    float amount = 1.0/GetDist(lightObj,lightObj.DistanceFunction,data.Pos)*lightAmount;

    amount = min(amount,1.0);//Making sure it isn't over 1
    amount = max(amount,0.0);//Making sure it is at least above or at 0





    //amount = min(amount,1.0);

    //color = vec4(mod(data.Pos/10.0,1.0),1);
    if (lightData.HitObject&&lightData.Index==1) {
      color = vec4(data.Color.xyz*amount,1);


      } else {

      //Attempt to get the last remaining color out of the distance before finally giving up.
      //color = vec4(0,0,0,1);

      //get distance to light source
      float amount = 1.0/GetDist(lightObj,lightObj.DistanceFunction,lightData.Pos)*lightAmount,1.0*lightFadeAmount;

      amount = min(amount,1.0);//Making sure it isn't over 1
      amount = max(amount,0.0);//Making sure it is at least above or at 0

      color = vec4(data.Color.xyz*amount,1);

    }

  } else {
    color = vec4(0,0,0,1);
  }


  //color = vec4(rayRot.x,rayRot.y,rayRot.z,1);

}

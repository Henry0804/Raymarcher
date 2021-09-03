#version 300 es
precision mediump float;
in vec2 screenSize;
out vec4 color;


float Dist(vec3 pos1,vec3 pos2) {
  return sqrt( (pos1.x-pos2.x)*(pos1.x-pos2.x) + (pos1.y-pos2.y)*(pos1.y-pos2.y) + (pos1.z-pos2.z)*(pos1.z-pos2.z) );
}

float SphereDist(vec3 spherePos,vec3 rayPos) {
  //vec3 dist = spherePos - rayPos;
  //vec3 dist = rayPos - spherePos;
  return Dist(spherePos,rayPos)-10.0;
}


vec4 MarchRay(vec3 pos2,vec3 dir2) {
  vec3 dir = normalize(dir2);
  vec3 pos = pos2;

  float dist = 999.0;
  float limit = 0.1;

  float length = 0.0;

  int maxCalls = 30;

  for (int calls = 0;calls<maxCalls; calls++) {
    dist = SphereDist(vec3(0.0,40.0,0.0),pos);

    //pos += dir*dist; //Old method

    //New method:

    length += dist;
    pos = dir*length;
    if (dist<=limit) {break;}
    //break;
  }

  if (dist<=limit) {return vec4(1,0,0,1);}
  return vec4(0,0,0,1);

}





void main() {
  vec3 Cam = vec3(0,0,0);

  //int width = 10;
  //int height = 10;

  //color = vec4(0.0, 1.0, 1.0, 1.0);

  vec2 range = vec2(1,1);

  vec2 sub5 = vec2(0.5,0.5);

  vec3 viewPos = vec3(gl_FragCoord.xy*2.0-1.0,1.0);

  vec3 camPos = vec3(0.0,0.0,0.0);


  vec3 rot = viewPos - camPos;

  rot = normalize(rot);


  color = MarchRay(camPos,rot);
  //color = vec4(screenSize.x/300.0+0.5,0.0,0.0,1.0);
  //color = vec4(gl_FragCoord.x/screenSize.x,gl_FragCoord.y/screenSize.y,0.0,1.0);
}

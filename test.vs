#version 300 es
precision mediump float;

uniform vec2 aScreenSize;
uniform vec3 aInput;

out vec3 input1;

out vec2 screenSize;

uniform vec3 aCamPos;
out vec3 camPos;


in vec2 pos;


void main() {

  input1 = vec3(aInput[0],aInput[1],aInput[2]);

  screenSize = vec2(aScreenSize[0],aScreenSize[1]);

  camPos = vec3(aCamPos[0],aCamPos[1],aCamPos[2]);

  gl_Position = vec4(pos,0,1);
}

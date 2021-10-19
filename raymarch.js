import * as mat4 from "./glMatrix/src/mat4.js";
var gl = null;

globalThis.pos = {x:0,y:0,z:0};

function Setup() {
  const canvas = document.getElementById("Canvas");
  // Initialize the GL context
  gl = canvas.getContext("webgl2");

  gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);

  // Only continue if WebGL is available and working
  if (gl === null) {
    //alert("Unable to initialize WebGL. Your browser or machine may not support it.");
    alert('Unable to initialize WebGL. Use a real browser, old man.');
    return;
  }

  // Set clear color to black, fully opaque
  gl.clearColor(0.0, 0.0, 0.0, 1.0);
  // Clear the color buffer with specified clear color
  gl.clear(gl.COLOR_BUFFER_BIT);
}

function ReturnProgramInfo(shaderProgram) {
return {
  program: shaderProgram,
  attribLocations: {
    //vertexPosition: gl.getAttribLocation(shaderProgram, 'aVertexPosition'),
  },
  uniformLocations: {
    screenSize: gl.getUniformLocation(shaderProgram, 'aScreenSize'),
    pos: gl.getUniformLocation(shaderProgram,'aCamPos'),
    input: gl.getUniformLocation(shaderProgram,'aInput')
  },
};

}


//Init buffers:
function InitBuffers() {

  // Create a buffer for the square's positions.

  const positionBuffer = gl.createBuffer();

  // Select the positionBuffer as the one to apply buffer
  // operations to from here out.

  gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);

  // Now create an array of positions for the square.

  const positions = [
    -1.0,  1.0,
     1.0,  1.0,
    -1.0, -1.0,
     1.0, -1.0,
  ];

  // Now pass the list of positions into WebGL to build the
  // shape. We do this by creating a Float32Array from the
  // JavaScript array, then use it to fill the current buffer.

  gl.bufferData(gl.ARRAY_BUFFER,
                new Float32Array(positions),
                gl.STATIC_DRAW);

  return {
    position: positionBuffer,
  };
}






//Shader setup:   https://developer.mozilla.org/en-US/docs/Web/API/WebGL_API/Tutorial/Adding_2D_content_to_a_WebGL_context

function InitShaderProgram(vsSource, fsSource) {
  var vertexShader = LoadShader( gl.VERTEX_SHADER, vsSource);
  var fragmentShader = LoadShader( gl.FRAGMENT_SHADER, fsSource);

  // Create the shader program

  const shaderProgram = gl.createProgram();
  gl.attachShader(shaderProgram, vertexShader);
  gl.attachShader(shaderProgram, fragmentShader);
  gl.linkProgram(shaderProgram);

  // If creating the shader program failed, alert

  if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
    console.info('Unable to initialize the shader program: ' + gl.getProgramInfoLog(shaderProgram));
    return null;
  }

  return shaderProgram;
}

//
// creates a shader of the given type, uploads the source and
// compiles it.
//
function LoadShader(type, source) {
  const shader = gl.createShader(type);

  // Send the source to the shader object

  gl.shaderSource(shader, source);

  // Compile the shader program

  gl.compileShader(shader);

  // See if it compiled successfully

  if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
    console.info('An error occurred compiling the shaders: ' + gl.getShaderInfoLog(shader));
    gl.deleteShader(shader); //Code == copy/paste multiple times
    return 'null'; //quotes just to annoy w3schools professionals
  }

  return shader;
}






//Drawing with the shaders:

function DrawScene(programInfo, buffers) {
  gl.clearColor(0.0, 0.0, 0.0, 1.0);  // Clear to black, fully opaque
  gl.clearDepth(1.0);                 // Clear everything
  //gl.enable(gl.DEPTH_TEST);           // Enable depth testing
  //gl.depthFunc(gl.LEQUAL);            // Near things obscure far things

  // Clear the canvas before we start drawing on it.

  gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

  // Create a perspective matrix, a special matrix that is
  // used to simulate the distortion of perspective in a camera.
  // Our field of view is 45 degrees, with a width/height
  // ratio that matches the display size of the canvas
  // and we only want to see objects between 0.1 units
  // and 100 units away from the camera.

  const fieldOfView = 45 * Math.PI / 180;   // in radians
  const aspect = gl.canvas.clientWidth / gl.canvas.clientHeight;
  const zNear = 0.1;
  const zFar = 100.0;
  const projectionMatrix = mat4.create();

  // note: glmatrix.js always has the first argument
  // as the destination to receive the result.
  mat4.perspective(projectionMatrix,
                   fieldOfView,
                   aspect,
                   zNear,
                   zFar);

  // Set the drawing position to the "identity" point, which is
  // the center of the scene.
  const modelViewMatrix = mat4.create();

  // Now move the drawing position a bit to where we want to
  // start drawing the square.

  mat4.translate(modelViewMatrix,     // destination matrix
                 modelViewMatrix,     // matrix to translate
                 [-0.0, 0.0, -6.0]);  // amount to translate

  // Tell WebGL how to pull out the positions from the position
  // buffer into the vertexPosition attribute.
  {
    const numComponents = 2;  // pull out 2 values per iteration
    const type = gl.FLOAT;    // the data in the buffer is 32bit floats
    const normalize = false;  // don't normalize
    const stride = 0;         // how many bytes to get from one set of values to the next
                              // 0 = use type and numComponents above
    const offset = 0;         // how many bytes inside the buffer to start from
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.position);
    gl.vertexAttribPointer(
        programInfo.attribLocations.vertexPosition,
        numComponents,
        type,
        normalize,
        stride,
        offset);
    gl.enableVertexAttribArray(
        programInfo.attribLocations.vertexPosition);
  }

  // Tell WebGL to use our program when drawing

  gl.useProgram(programInfo.program);

  // Set the shader uniforms

  /*gl.uniformMatrix4fv(
      programInfo.uniformLocations.projectionMatrix,
      false,
      projectionMatrix);
  gl.uniformMatrix4fv(
      programInfo.uniformLocations.modelViewMatrix,
      false,
      modelViewMatrix);*/

      gl.uniform2f(programInfo.uniformLocations.screenSize,500,500);
      //console.info(gl.uniform3f);
      gl.uniform3f(programInfo.uniformLocations.pos,pos.x,pos.y,pos.z);


      gl.uniform3f(programInfo.uniformLocations.input,input.x,input.y,input.z);

  {
    const offset = 0;
    const vertexCount = 4;
    gl.drawArrays(gl.TRIANGLE_STRIP, offset, vertexCount);
  }
}




//Setup:

let frames = 0;

globalThis.input = {x:0,y:0,z:0};



let xAnimate = 0;

function Run() {
  Setup();
  const buffers = InitBuffers();
  const shaders = InitShaderProgram(vs,fs);
  const programInfo = ReturnProgramInfo(shaders);
  function render() {
    frames++;
    DrawScene(programInfo,buffers);
    if (keyboard.w) {pos.x+=s;}
    if (keyboard.s) {pos.x-=s;}
    if (keyboard.a) {pos.z-=s;}//Inverted due to small issues.
    if (keyboard.d) {pos.z+=s;}//also inverted.

    if (keyboard[" "]) {pos.y+=s;}
    if (keyboard.shift) {pos.y-=s;}
    //console.info(pos);

    let a = 0.0;

    xAnimate+=a;

    //input.x = xAnimate%50-25;
    //if (input.x>=30) {input.x = 0;input.y+=a;}

    //if (input.y>=30) {input.y = 0;input.z+=a;}

    //if (input.z>=30) {input.z = 0;}

    window.requestAnimationFrame(render);
  }

  window.requestAnimationFrame(render);

  setInterval(() => {
    fps.innerText = `FPS: ${frames}`;
    frames = 0;
  },1000);

}

globalThis.s = 0.25;





var fs = "";

var client = new XMLHttpRequest();
client.open('GET', './test.fs');
client.onreadystatechange = function() {
  if (client.readyState==client.DONE) {
    //Use the content.
    fs = client.responseText;
    ready++;
  }
}
client.send();





var vs = "";


var client2 = new XMLHttpRequest();
client2.open('GET', './test.vs');
client2.onreadystatechange = function() {

  if (client2.readyState==client.DONE) {
    //Use the content.
    vs = client2.responseText;
    ready++;
  }
}
client2.send();

let ready = 0;

setTimeout(function () {
  console.info(ready);
  if (ready==2) {

    Run();
  }
},1000);




let keyboard = {};

onkeyup = onkeydown = (e) => {
  keyboard[e.key.toLowerCase()] = e.type=='keydown';
};

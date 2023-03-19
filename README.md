# Weighted-Blended-OIT-in-Unity-URP
This project is to implement Weighted Blended OIT in Unity URP.I do this job in order to solve the transparency sorting problem when I do hair shading.


Reference:

[paper-lowres.pdf](https://github.com/HigashiSan/Weighted-Blended-OIT-in-Unity-URP/files/10974002/paper-lowres.pdf)
![image](https://user-images.githubusercontent.com/56297955/225170769-d0a305b8-f2c0-495d-917a-5135ed860bb9.png)


Core idea:

![image](https://user-images.githubusercontent.com/56297955/225149597-95f29a0d-4470-43de-8cac-c5c4cf3a0e9a.png)


Four weight functions:

![image](https://user-images.githubusercontent.com/56297955/225148444-61f9a513-1bee-4978-9f04-ec167ad3df83.png)

### How to do
First using alpha and depth to determind how much color it contribute(Ci), then let the albedo(basic color) multiply by Ci. Use linear addition to write the color contribution to the accumTexture. 

Second using **Blend 1 Zero OneMinusSrcAlpha** blend order to write alpha to revealageTexture. Its main idea is that by subtracting alpha in turn, you can assume that the alpha of each translucent object affects the color of the background, and the more alpha accumulates, the greater the effect.

Finally using these two texture to blend with background:

![image](https://user-images.githubusercontent.com/56297955/226152483-fbf93649-2456-4a4f-8fa9-28bba098881f.png)


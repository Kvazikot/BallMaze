# BallMaze
This project implements a path planning algorithm for a car-like robot that travels through a maze.

Vladimir Baranov (vsbaranov83@gmail.com)
homepage https://sites.google.com/site/midanedev/3d-labirint

for Unity 2019.3.11f1

RRT path planning algorithm.
(Part of the car_model Unity project.)

Algorithmic task: investigate the RRT algorithm for control to bypass collisions and go the route from the starting point, through intermediate points and to the final
Rapidly-exploring Random Trees algorithm. (see article PlanningforDynamicVeh-1.pdf)

Rrt_planer.cs program module
In the first step, a simple tree algorithm can be implemented without Runge-Kutta path integration (as described in LavKuf01.pdf). Figure 2.
For many obstacles, you can use something similar to this picture.
In units, this means that instead of lines, you need to draw extended objects such as parallelepipeds.
The transition to the maze will take place at later iterations of the project.
In the first stages, I will focus on building a graph and drawing a trajectory in the form of lines and not think about the control object yet. Then connect the kinematic model of the car to calculate the turns. Draw paths as curves. When the path planning algorithm works, it will be possible to connect vehicle control scripts. As an input, take the control graph built using the RRT algorithm (rrt_planer.cs)

Algorithmic questions:
How to choose distribution in random_config. Function (goal biased or uniform approach)?
In the new_state function, what to choose for u the speed or the angle of rotation of the wheels, both?
From which set to choose speeds or angles for each call to new_state?
What is the criterion for choosing one specific trajectory from the set of all possible in the graph?

What do I dislike about this picture?
First, the trajectory changes too abruptly. This is presumably due to the random nature of the steering function (see next heading)
Second, there are defects in the collision detector function.
We need a smooth tack noise function with cubic spline interpolation.

Distribution test.
    1. Connect TestDll to the RttPlaner.cs script and make one GenerateCoordinates delegate
    2. Prepare the test in RttPlaner.cs the test will send to TestDll the distribution parameters maximum and minimum values ​​for the X and Z components. At the output, the c ++ code will generate two arrays of type double with coordinates
    3. To check the correct generation of coordinates, you can create spheres of a point size in coordinates from these two arrays

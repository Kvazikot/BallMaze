# Component "Rrt Planer"

## Description:
   Make agent way through obstacles (for example labirynth) result 

## Input Parametres
* "Start P Object" - Object (usually sphere) represents starting point of path
* "End P Object" - Object that represents destination point
* "Velocity" - velocity of kinematic model (increase proportional to maze scale)
* "K_iterations" - number of iterations of rrt planer algorithm (corresponds to number of generated points in graph)

## Output:   
   Trajectory from "Start P Object" to "End P Object" with many of red spheres (waypoints) on leaves
____________________________________________________________________________________________

# Component "MazeGen"

## Description:
   Generate maze with walls as unity cube primitives. 
   Note: Attach this script to plane object.

## Input Parametres
* CellsX - x resolution
* CellsY - y resolution
* Wall_width - scale of horizontal and vertical primitives


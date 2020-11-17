В этом проекте реализован алгоритм планирования пути для car-like робота который едет через лабиринт.
Баранов Владимир (vsbaranov83@gmail.com)
для Unity 2019.3.11f1

Алгоритм планирования пути RRT. 
(Входит в состав Unity-проекта car_model.)

Алгоритмическая задача: исследовать алгоритм RRT для управления чтобы обходить коллизии и проходить маршрут от начальной точки, через промежуточные и до конечной
Алгоритм Rapidly-exploring Random Trees. (см. статью PlanningforDynamicVeh-1.pdf)

Програмный модуль rrt_planer.cs
See demo here https://youtu.be/O0O-UNEF0ks

На первом этапе можно реализовать алгоритм простого дерева без интегрирования траектории по Рунге-Кутта (как описано в LavKuf01.pdf).  Figure 2.
В качестве множества препятствий можно использовать что-то похожее на этот рисунок.
В юнити это означает что нужно вместо линий нарисовать протяженные объекты типа параллепипедов. 
Переход к лабиринту осуществится на поздних итерациях проекта.
На первых этапах сосредоточится на построении графа и отрисовке траектории в виде линий и не думать пока о объекте управления. Потом подключить кинематическую модель машины для расчета поворотов. Построить траектории в виде кривых. Когда будет работать алгоритм планирования пути можно будет подключать скрипты управления машиной . В качестве входа взять граф управления построенный по алгоритму RRT (rrt_planer.cs)

Вопросы по алгоритмической части: 
Как выбирать распределение в функции random_config.(подход goal biased или uniform)?
В функции new_state что выбирать за u скорость или угол поворота колес, и то и другое?
Из какого множества выбирать скорости или углы при каждом вызове new_state?
По какому критерию выбирать одну конкретную траекторию из множества всех возможных в графе?

Что мне не нравится в этой картинке?
Во первых траектория меняется слишком резко. Это предположительно изза того что функция steering имеет случайный характер (см. следующий заголовок)
Bторое есть дефекты в функции детектора коллизий.
Нам нужна функция сглаженного галсового шума с кубической сплайновой интерполяцией.

Тест распределения.
    1. Подключить TestDll к скрипту RttPlaner.cs и сделать один делегат GenerateCoordinates
    2. Подготовить тест в RttPlaner.cs тест будет отправлять в TestDll параметры распределения максимальные и минимальные значения для X и Z компоненты. На выходе c++ код сгенерирует два массива типа double с координатами
    3. Чтобы проверить правильность генерации координат можно создать сферы точечного размера в координатах из этих двух массивов

=======
# BallMaze
This project implements a path planning algorithm for a car-like robot that travels through a maze.

Vladimir Baranov (vsbaranov83@gmail.com)
homepage https://sites.google.com/site/midanedev/3d-labirint

for Unity 2019.3.11f1

RRT path planning algorithm.
(Part of the car_model Unity project.)
See demo here https://youtu.be/O0O-UNEF0ks

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


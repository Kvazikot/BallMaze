# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import numpy as np
import matplotlib.pyplot as plt

u, v = np.mgrid[0:2*np.pi:20j, 0:np.pi:10j]
m = 1
r = 2*m + v*v/8*m
x = r * np.cos(u)
y = r * np.sin(u)
z = v



fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.plot_wireframe(x, y, z)
ax.legend()


#include <algorithm>
//#include <Windows.h>
#include <IUnityInterface.h>
#include <string.h>
#include <iostream>
#include <fstream>
#include <array>
#include <random>
#include <map>
#include <vector>
#define _USE_MATH_DEFINES
#include <math.h>

#define PLUGINEX(rtype) UNITY_INTERFACE_EXPORT rtype UNITY_INTERFACE_API

inline float middle(float xs, float xe)
{
	return (std::min(xs, xe) + (std::max(xs, xe) - std::min(xs, xe)) / 2);
}

struct Vector2
{
	float x;
	float y;
	Vector2() { x = 0; y = 0;  }
	Vector2(float _x, float _y)
		:x(_x), y(_y)
	{
	}
};

extern "C" {

	
	inline Vector2 circle_sample(Vector2 c, float random_phi, float random_R)
	{
		Vector2 sample;

		sample.x = c.x + random_R * cos(random_phi);
		sample.y = c.y + random_R * sin(random_phi);

		return sample;
	}

	inline bool isPointInsideArea(Vector2 p, float xmin, float xmax, float ymin, float ymax)
	{
		return (p.x >= xmin) && (p.x <= xmax) && (p.y >= ymin) && (p.y <= ymax);
	}


	PLUGINEX(int) GenerateCoordinates(float Xs[], float Ys[], int len, float xs, float xe, float ys, float ye)
	{
		std::default_random_engine generator;
		float Rmax = sqrt((xe - xs) * (xe - xs) + (ye - ys) * (ye - ys));

		std::array<double, 3> intervalsR{ 0, Rmax/2,  Rmax };
		std::array<double, 3> weightsR{ 15, 10, 5 };
		std::array<double, 3> intervalsPHI{ 0, 2 * M_PI };
		std::array<double, 3> weightsPHI{ 10, 10 };

		std::piecewise_linear_distribution<double>
			distributionR(intervalsR.begin(), intervalsR.end(), weightsR.begin());

		std::piecewise_linear_distribution<double>
			distributionPHI(intervalsPHI.begin(), intervalsPHI.end(), weightsPHI.begin());

		int n = 0; int max_iters = 100000000;
		while (n < len)
		{
			float r = distributionR(generator);
			float phi = distributionPHI(generator);
			Vector2 point = circle_sample(Vector2(xe, ye), phi, r);
			if (isPointInsideArea(point, std::min(xs, xe), std::max(xs, xe), std::min(ys, ye), std::max(ys, ye)))
			{
				Xs[n] = point.x;
				Ys[n] = point.y;
				n++;
			}
			max_iters--;
			if (max_iters == 0) return n;
		}
		return n;
	}

	PLUGINEX(int) GenerateCoordinates2(float Xs[], float Ys[], int len, float xs, float xe, float ys, float ye)
	{
		std::default_random_engine generator;
		std::array<double, 3> intervalsX{ xs, middle(xs, xe),  xe };
		std::array<double, 3> intervalsY{ ys, middle(ys, ye), ye };
		std::array<double, 3> weights{ 2, 4, 15.0 };

		std::piecewise_linear_distribution<double>
			distributionX(intervalsX.begin(), intervalsX.end(), weights.begin());

		std::piecewise_linear_distribution<double>
			distributionY(intervalsY.begin(), intervalsY.end(), weights.begin());

		for (int i = 0; i < len; i++) {
			Xs[i] = distributionX(generator);
			Ys[i] = distributionY(generator);

		}

		return len;

	}

	Vector2 fromPixelsToWorld(Vector2 p, int maze_size, int image_size)
	{
		float units_in_pixel = (float)maze_size / (float)image_size;
		float x = ((float)(image_size - p.x - 1)) * units_in_pixel - maze_size / 2;
		float z = ((float)(image_size - p.y - 1)) * units_in_pixel - maze_size / 2;
		return Vector2(x,z);
	}


	PLUGINEX(int) GenerateCoordinatesDist(float Xs[], float Ys[], int Colors[], int n_points, int distImage[], int image_size, int maze_size)
	{
		std::map<int, std::vector<int> > dist_map;
		std::map<int, int> index_conv_tab;
		std::vector<int> distToEnd;
		
		std::ofstream fout;
		fout.open("GenerateCoordinatesDist.txt"); // связываем объект с файлом

		distToEnd.resize(image_size*image_size);
		for (int i = 0; i < image_size*image_size; i++)
		{
			int x = i % image_size;
			int y = i / image_size;
			int x2 = 0;
			int y2 = image_size;
			distToEnd[i] = (int)sqrtf((x2 - x)*(x2 - x) + (y2 - y)*(y2 - y));
			//fout << distToEnd[i] << ",";
			//if (x == 0)
			//	fout << "\n";
		}
		

		for (int i = 0; i < image_size*image_size; i++)
		{
			int d_key = distImage[i];
			if (dist_map.find(d_key) != dist_map.end())
			{
				dist_map[d_key].push_back(i);
			}
			else
				dist_map[d_key] = std::vector<int>();
		}
		// Calc statistics on distance image
		// sort vectors by distance to end point
		int idx = 0;
		for (auto it = dist_map.begin(); it != dist_map.end(); it++)
		{
			index_conv_tab[idx++] = it->first;
			fout << it->first << "-" << it->second.size() << "\n";
			std::sort(it->second.begin(), it->second.end(), [&](int idx1, int idx2) -> bool
			{
				return distToEnd[idx1] < distToEnd[idx2];
			}
			);
		}

		

		// Testing old distribution (should work as GenerateCoordinates2)
		std::default_random_engine generator;

		

		std::uniform_int_distribution<int> uniform_distribution(2, dist_map.size()-1);
		/*
		for (int i = 0; i < n_points; i++) {

			int n1 = index_conv_tab[uniform_distribution(generator)];
			

			std::array<int, 2> intervals{ 0, dist_map[n1].size() };
			std::array<int, 2> weights{ 0, 1 };
			std::piecewise_linear_distribution<float>
				distributionX(intervals.begin(), intervals.end(), weights.begin());
			int selected_index = dist_map[n1][distributionX(generator)];
			int x = selected_index % image_size;
			int y = selected_index / image_size;
			Vector2 p_in(x,y);
			Vector2 p = fromPixelsToWorld(p_in, maze_size, image_size);
			Xs[i] = p.x;
			Ys[i] = p.y;
			//Colors[i] = distToEnd[selected_index];

		}*/
		int xs = 0;
		int xe = image_size - 1;
		int ys = 0;
		int ye = image_size - 1;
		std::array<double, 3> intervalsX{ xs, middle(xs, xe),  xe };
		std::array<double, 3> intervalsY{ ys, middle(ys, ye), ye };
		std::array<double, 3> weights{ 2, 4, 15.0 };

		std::piecewise_linear_distribution<double>
			distributionX(intervalsX.begin(), intervalsX.end(), weights.begin());

		std::piecewise_linear_distribution<double>
			distributionY(intervalsY.begin(), intervalsY.end(), weights.begin());

	
		int n = 0;
		for (n = 0; n < n_points; n++)
		{
			int i = distributionX(generator);
			int j = distributionY(generator);
			int d_key = distImage[j*image_size + i];
			if (d_key > 2)
			{
				Vector2 p_in(i, j);
				Vector2 p = fromPixelsToWorld(p_in, maze_size, image_size);
				Xs[n] = p.x;
				Ys[n] = p.y;
				//fout << p.x << " " << p.y << "\n";
				n++;
			}
			
		}
		fout.close();
		return n_points;
	}
}

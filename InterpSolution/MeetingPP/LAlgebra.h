#pragma once

#include"stdafx.h"
#include "Funcs.h"

#define dot(u,v) ((u).x * (v).x + (u).y * (v).y + (u).z * (v).z)
#define len(v) sqrt(dot((v),(v)))
#define cross(u,v) Vec3((u).y*(v).z - (u).z*(v).y,(u).z*(v).x - (u).x*(v).z,(u).x*(v).y - (u).y*(v).x)
#define add(u,v) Vec3((u).x+(v).x,(u).y+(v).y,(u).z+(v).z)
#define sub(u,v) Vec3((u).x-(v).x,(u).y-(v).y,(u).z-(v).z)
#define mul(v,a) Vec3((v).x*(a),(v).y*(a),(v).z*(a))
#define dev(v,b) Vec3((v).x/(b),(v).y/(b),(v).z/(b))
#define norm(v) dev((v),len((v)))
#define dist(u,v) len(sub(u,v))
#define GRAD 57.295779513
#define RAD 0.01745329252
#define angle_rad(u,v) acos(dot(norm(u),norm(v)))
#define angle_gr(u,v) angle_rad(u,v)*GRAD
#define SMALL_NUM   0.00000001

struct Vec3 {
public:
	TFloat x, y, z;
	Vec3()
	{
		x = 0;
		y = 0;
		z = 0;
	}
	Vec3(const TFloat xp, const  TFloat yp, const  TFloat zp):x(xp),y(yp),z(zp)
	{
	}

	inline TFloat GetLength() const{
		return sqrt(x*x + y*y + z*z);
	}

	Vec3& operator=(const Vec3& a) {
		x = a.x;
		y = a.y;
		z = a.z;                                                                                                                                      
		return *this;
	}
	bool operator == (const Vec3& a) const{
		return abs(x - a.x) < SMALL_NUM && abs(y - a.y) < SMALL_NUM && abs(z - a.z) < SMALL_NUM;
	}

	Vec3 operator+(const Vec3& a) const {
		return Vec3(a.x + x, a.y + y, a.z + z);
	}
	Vec3 operator-(const Vec3& a) const {
		return Vec3(x - a.x, y - a.y, z - a.z);
	}
	Vec3 operator*(const TFloat& a) const {
		return Vec3(a* x, a*y, a*z);
	}
	Vec3 operator/(const TFloat& a) const {
		return Vec3(x/a, y/a, z/a);
	}

};




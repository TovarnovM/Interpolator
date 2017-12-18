#pragma once

#include"Funcs.h"
#include<math.h>

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
#define BIG_NUM 1E13

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
		return fabs(x - a.x) < SMALL_NUM && fabs(y - a.y) < SMALL_NUM && fabs(z - a.z) < SMALL_NUM;
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
	inline Vec3 Norm() const {
		TFloat length = GetLength();
		return Vec3(x / length, y / length, z / length);
	}
};


void GoToNextPos(const Vec3 fromPos_Pos, const Vec3 fromPos_Vel, const Vec3 posFromGranny_Pos, const Vec3 posFromGranny_Vel, Vec3& res_Pos, Vec3& res_Vel);
inline TFloat Get_r1(const Vec3 &P0, const  Vec3& P1, const  Vec3 &V0, const  Vec3 &n) {
	TFloat znam = dot(n, (P1 - P0));
	if (fabs(znam) < SMALL_NUM) {
		return BIG_NUM;
	}
	return dot(n, (V0 - P0)) / znam;
}
void AngleToSegmentFromRay(const Vec3& ray_p, const Vec3 &ray_dir, const Vec3 &seg_p1, const Vec3 &seg_p2, TFloat& out_angle, Vec3 &out_closestP);
void DistanceToSegment(const Vec3& p, const  Vec3 &lp1, const  Vec3 &lp2, TFloat & out_d, Vec3 &out_closestP);
inline int sign(const TFloat &val) {
	return val < 0 ? -1 : 1;
}
Vec3 GetSurfTarget_toPoint(const Vec3& n, const Vec3& p_surf, const Vec3& vel, const Vec3& pos, const Vec3& toPoint, const TFloat gamma = 30, const TFloat dt = 1.0 / 23.0, const TFloat a = 13);


#include "LAlgebra.h"

void GoToNextPos(const Vec3 fromPos_Pos, const Vec3 fromPos_Vel, const Vec3 posFromGranny_Pos, const Vec3 posFromGranny_Vel, Vec3 & res_Pos, Vec3 & res_Vel)
{
	Vec3 xl = Vec3(fromPos_Vel.x, 0, fromPos_Vel.z).Norm();
	Vec3 yl(0, 1, 0);
	Vec3 zl = cross(xl, yl);

	Vec3 xg(dot(Vec3(1, 0, 0), xl), 0, dot(Vec3(1, 0, 0), zl));
	Vec3 yg(0, 1, 0);
	Vec3 zg(dot(Vec3(0, 0, 1), xl), 0, dot(Vec3(0, 0, 1), zl));

	res_Pos = fromPos_Pos + Vec3(dot(posFromGranny_Pos, xg) , dot(posFromGranny_Pos, yg), dot(posFromGranny_Pos, zg));
	res_Vel = Vec3(dot(posFromGranny_Vel, xg), dot(posFromGranny_Vel, yg), dot(posFromGranny_Vel, zg));
}

void AngleToSegmentFromRay(const Vec3 & ray_p, const Vec3 & ray_dir, const Vec3 & seg_p1, const  Vec3 & seg_p2, TFloat & out_angle, Vec3 & out_closestP)
{
	Vec3 u = ray_dir;
	Vec3 v = seg_p2 - seg_p1;

	Vec3 w0 = ray_p - seg_p1;

	TFloat a = dot(u, u);
	TFloat b = dot(u, v);
	TFloat c = dot(v, v);
	TFloat d = dot(u, w0);
	TFloat e = dot(v, w0);

	TFloat s_c = (b * e - c * d) / (a * c - b * b);
	TFloat t_c = (a * e - b * d) / (a * c - b * b);

	if (s_c < 0) {
		s_c = 0;
	}
	Vec3 p_on_ray = ray_p + ray_dir * s_c;

	if (t_c < 0) {
		t_c = 0;
	}
	else if (t_c > 1) {
		t_c = 1;
	}

	Vec3 p_on_seg = seg_p1 + v*t_c;
	TFloat angleCos = dot(ray_dir.Norm(), (p_on_seg - ray_p).Norm());
	if (angleCos > 1) {
		angleCos = 1;
	}
	else if (angleCos < -1) {
		angleCos = -a;
	}
	out_angle = acos(angleCos);
	out_closestP = p_on_seg;

}

void DistanceToSegment(const Vec3 & p, const Vec3 & lp1, const Vec3 & lp2, TFloat & out_d, Vec3 & out_closestP)
{
	Vec3 v = lp2 - lp1;
	Vec3 w = p - lp1;

	TFloat c1 = dot(w, v);
	if (c1 <= 0) {
		out_d = (p - lp1).GetLength();
		out_closestP = lp1;
		return;
	}



	TFloat c2 = dot(v, v);
	if (c2 <= c1) {
		out_d = (p - lp2).GetLength();
		out_closestP = lp2;
		return;
	}


	TFloat b = c1 / c2;
	Vec3 Pb = lp1 + v * b;
	out_d = (p - Pb).GetLength();
	out_closestP = Pb;
}

Vec3 GetSurfTarget_toPoint(const Vec3 & n, const Vec3 & p_surf, const Vec3 & vel, const Vec3 & pos, const Vec3 & toPoint, const TFloat gamma, const TFloat dt, const TFloat a)
{
	Vec3 p_loc1 = pos - p_surf;
	Vec3 pos_s = pos - n * dot(p_loc1, n);
	Vec3 diffpos = pos - pos_s;

	Vec3 y_s = diffpos.GetLength() < SMALL_NUM ? n :diffpos.Norm();
	Vec3 vel_ys = y_s * dot(y_s, vel);
	Vec3 vel_xs = vel - vel_ys;
	Vec3 x_s = vel_xs.Norm();
	Vec3 z_s = cross(x_s, y_s);

	TFloat l_s = vel.GetLength() * dt * a;

	Vec3 toPoint_s = toPoint - pos_s;
	Vec3 toPoint_s_1 = toPoint_s.Norm();

	if (l_s > (pos - toPoint).GetLength()) {
		return toPoint;
	}

	TFloat gamma_toPoint = atan2(dot(toPoint_s_1, z_s), dot(toPoint_s_1, x_s)) * GRAD;
	TFloat gamma2 = 0;
	if (fabs(gamma_toPoint) > gamma) {
		gamma2 = fabs(gamma) * sign(gamma_toPoint);
	}
	else {
		gamma2 = gamma_toPoint;
	}

	Vec3 r_s = x_s * cos(gamma2 * RAD) + z_s * sin(gamma2 * RAD);

	Vec3 p_dist_s = pos_s + r_s * l_s;
	return p_dist_s;
}

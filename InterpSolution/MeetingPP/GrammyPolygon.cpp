#include "GrammyPolygon.h"

bool GrammyPolygon::IsCross(const Vec3 & p_ray, const  Vec3 & ray_dir, Vec3 & cross_p, TFloat & dist) const
{
	Vec3 n = (cross((p2 - p1).Norm(), (p3 - p1).Norm())).Norm();
	TFloat r1 = Get_r1(p_ray, p_ray + ray_dir, p1, n);
	if (r1 >= BIG_NUM) {
		dist = BIG_NUM;
		return false;
	}

	Vec3 cross_plane = p_ray + ray_dir * r1;

	Vec3 u = p2 - p1;
	Vec3 v = p3 - p1;
	Vec3 w = cross_plane - p1;

	TFloat s1 = (dot(u, v) * dot(w, v) - dot(v, v) * dot(w, u)) / (dot(u, v) * dot(u, v) - dot(u, u) * dot(v, v));
	TFloat t1 = (dot(u, v) * dot(w, u) - dot(u, u) * dot(v, v)) / (dot(u, v) * dot(u, v) - dot(u, u) * dot(v, v));

	bool intersect = (s1 >= 0 && t1 >= 0 && t1 + s1 <= 1);
	if (!intersect) {
		TFloat d1, d2, d3;
		Vec3 cp1, cp2, cp3;
		AngleToSegmentFromRay(p_ray, ray_dir, p1, p2, d1, cp1);
		AngleToSegmentFromRay(p_ray, ray_dir, p2, p3, d2, cp2);
		AngleToSegmentFromRay(p_ray, ray_dir, p1, p3, d3, cp3);
		if (d1 < d2) {
			if (d1 < d3) {
				cross_p = cp1;
				dist = d1;
			}
			else {
				cross_p = cp3;
				dist = d3;
			}
		}
		else {
			if (d2 < d3) {
				cross_p = cp2;
				dist = d2;
			}
			else {
				cross_p = cp3;
				dist = d3;
			}
		}

	}
	else {
		cross_p = cross_plane;
		dist = 0;
	}
	return intersect;
}

TFloat * GrammyPolygon::InterpV(const Vec3 & p_glob, const size_t vecLength) const
{
	Vec3 x1 = (p2 - p1).Norm();
	Vec3 z1 = (cross(x1, (p3 - p1))).Norm();
	Vec3 y1 = cross(z1, x1);

	Vec3 pa(0, 0, 0);
	Vec3 pb(dot((p2 - p1), x1), dot((p2 - p1), y1), 0);
	Vec3 pc(dot((p3 - p1), x1), dot((p3 - p1), y1), 0);

	Vec3 p(dot(x1, (p_glob - p1)), dot(y1, (p_glob - p1)), 0);

	TFloat alf = ((p.x - pb.x) * (pc.y - pb.y) - (pc.x - pb.x) * (p.y - pb.y)) / ((pc.y - pb.y) * (pa.x - pb.x) -
		(pc.x - pb.x) * (pa.y - pb.y));
	TFloat bet = ((p.y - pb.y) * (pa.x - pb.x) - (p.x - pb.x) * (pa.y - pb.y)) / ((pc.y - pb.y) * (pa.x - pb.x) -
		(pc.x - pb.x) * (pa.y - pb.y));
	TFloat * res = new TFloat[vecLength];
	for (size_t i = 0; i < vecLength; i++)
	{
		res[i] = alf * (v1[i] - v2[i]) + bet * (v3[i] - v2[i]) + v2[i];
	}
	return res;
}


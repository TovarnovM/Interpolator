#pragma once
#include "Funcs.h"
#include "LAlgebra.h"

class GrammyPolygon {
public:
	Vec3 p1, p2, p3;
	TFloat *v1, *v2, *v3;
	GrammyPolygon() {

	}
	GrammyPolygon(TFloat *av1, TFloat *av2, TFloat *av3) :p1(av1[6], av1[7], av1[8]), p2(av2[6], av2[7], av2[8]), p3(av3[6], av3[7], av3[8])
	{
		v1 = av1;
		v2 = av2;
		v3 = av3;
	}
	bool IsCross(const Vec3& p_ray, const Vec3& ray_dir, Vec3& cross_p, TFloat & dist) const;
	TFloat* InterpV(const Vec3 &p_glob, const size_t vecLength) const;

};


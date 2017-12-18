#pragma once

#include"Funcs.h"
#include"GrammyPolygon.h"

class Grammy
{
public:
	const static size_t vecBeginLength = 6;
	const static size_t vecLength = 12;
	const static size_t polyCount = 2;
	TFloat vBegin[vecBeginLength],
		vUp[vecLength],
		vDown[vecLength],
		vLeft[vecLength],
		vRight[vecLength],
		vCenter[vecLength];
	GrammyPolygon polygons[polyCount];
	Grammy(TFloat* big_vec);
	~Grammy();
	void IntiPolygons() {
		polygons[0] = GrammyPolygon(vRight, vUp, vLeft);
		polygons[1] = GrammyPolygon(vRight, vDown, vLeft);
	}

	TFloat * PolygonsIntercept(const Vec3& ray_p, const Vec3 &ray_dir) const;
};

inline void PosFromVec(TFloat* vec, Vec3 &pos, Vec3 &vel) {
	pos.x = vec[6];
	pos.y = vec[7];
	pos.z = vec[8];
	vel.x = vec[9];
	vel.y = vec[10];
	vel.z = vec[11];
}
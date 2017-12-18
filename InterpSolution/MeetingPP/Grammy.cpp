#include "Grammy.h"



Grammy::Grammy(TFloat* big_vec)
{
	size_t i = 0;
	for (size_t j = 0; j < vecBeginLength; j++) {
		vBegin[j] = big_vec[i];
		i++;
	}
	for (size_t j = 0; j < vecLength; j++) {
		vUp[j] = big_vec[i];
		i++;
	}
	for (size_t j = 0; j < vecLength; j++) {
		vDown[j] = big_vec[i];
		i++;
	}
	for (size_t j = 0; j < vecLength; j++) {
		vLeft[j] = big_vec[i];
		i++;
	}
	for (size_t j = 0; j < vecLength; j++) {
		vRight[j] = big_vec[i];
		i++;
	}
	for (size_t j = 0; j < vecLength; j++) {
		vCenter[j] = big_vec[i];
		i++;
	}
	IntiPolygons();
}


Grammy::~Grammy()
{
}

TFloat * Grammy::PolygonsIntercept(const Vec3 & ray_p, const Vec3 & ray_dir) const
{
	TFloat minDist = BIG_NUM;
	Vec3 minDist_p(0, 0, 0);
	size_t min_dist_index = 0;
	for (size_t i = 0; i < polyCount; i++) {
		Vec3 minDist_i(0, 0, 0);
		TFloat dist_i = 0;
		if (polygons[i].IsCross(ray_p, ray_dir, minDist_i, dist_i)) {
			minDist_p = minDist_i;
			min_dist_index = i;
			break;
		}
		if (dist_i < minDist) {
			minDist_p = minDist_i;
			minDist = dist_i;
			min_dist_index = i;
		}
	}
	TFloat * dopVec = polygons[min_dist_index].InterpV(minDist_p, vecLength);
	return dopVec;
}

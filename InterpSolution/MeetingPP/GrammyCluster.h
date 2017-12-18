#pragma once

#include"stdafx.h"
#include"Funcs.h"
#include"LAlgebra.h"
#include"Grammy.h"

class GrammyCluster {
public:
	TFloat *dems_dx;
	TFloat **dems;
	TFloat *data_arr;
	size_t* dems_length;
	size_t* dems_4i;
	size_t n_elems;

	TFloat par_tFast_base_part;
	TFloat par_extrime_dL;
	TFloat par_h0;
	TFloat par_dt;
	TFloat par_gst_dist;
	TFloat par_gamma;
	TFloat par_a;

	GrammyCluster();
	~GrammyCluster();
	int load_from_csv(const char* file_name);
	int load_from_bin(const char* file_name);
	int save_to_bin(const char* file_name);
	TFloat * get_vec(const size_t* inds);
	TFloat * get_vec(	const size_t i0,
						const size_t i1,
						const size_t i2,
						const size_t i3,
						const size_t i4,
						const size_t i5) const;

	void GetInterp(const size_t inds_n, const TFloat val, size_t &out_ind0, size_t &out_ind1, TFloat &out_t) const;
	TFloat* GrammyVecInterp(TFloat *vBegin) const;
	inline TFloat * VecInterp(TFloat * v1, TFloat * v2, const TFloat t12, const size_t length = 66) const;

	void GrammyStep_ray(Vec3& dist_ray,
						Vec3& currPos_Pos,
						Vec3& currPos_Vel,
						TFloat* currVec,
						TFloat &grammyL) const;

	inline TFloat GrammyStep_toPoint(Vec3& to_point,
						Vec3& currPos_Pos,
						Vec3& currPos_Vel,
						TFloat* currVec,
						TFloat &grammyL) const {
		Vec3 rayDist = (to_point - currPos_Pos).Norm();
		GrammyStep_ray(rayDist, currPos_Pos, currPos_Vel, currVec, grammyL);
		return (to_point - currPos_Pos).GetLength();
	}

	inline bool GoodVec(TFloat *vec) const {
		return !(dems[2][0] > vec[2] || vec[2] > dems[2][dems_length[2] - 1] //Vel
			|| dems[5][0] > vec[5] || vec[5] > dems[5][dems_length[5] - 1] //Thetta
			);
	}

	bool HitFromThatPos(Vec3 pos0_pos,
		Vec3 pos0_vel,
		TFloat* vec0,
		Vec3 & p_dist,
		Vec3 &surf_n,
		Vec3 &surf_p,
		TFloat t_max,
		Vec3* lst_pos,
		Vec3* lst_vel,
		TFloat** lst_vec,
		size_t &count,
		TFloat dt = 1.0 / 23.0,
		TFloat h0 = -20,
		TFloat  gst_dist = 1800,
		TFloat gamma = 30,
		TFloat a = 13);

	bool HitFromThatPos(Vec3 pos0_pos,
		Vec3 pos0_vel, 
		TFloat* vec0,
		Vec3 & p_dist,
		TFloat t_max, 
		Vec3* lst_pos,
		Vec3* lst_vel,
		TFloat** lst_vec,
		size_t &count,
		TFloat dt = 1.0 / 23.0,
		TFloat h0 = -20,
		TFloat  gst_dist = 1800,
		TFloat gamma = 30,
		TFloat a = 13) 
	
	{
		Vec3 p_in_surf = pos0_pos - pos0_vel*100;
		Vec3 p_in_surf2 = p_in_surf + pos0_vel*100;

		Vec3 surf_n = (cross((p_in_surf2 - p_in_surf).Norm(), (p_dist - p_in_surf).Norm())).Norm();
		
		return HitFromThatPos(pos0_pos, pos0_vel, vec0, p_dist, surf_n, p_in_surf, t_max, lst_pos, lst_vel, lst_vec, count, dt, h0, gst_dist,gamma,a);
	}

	Vec3* GetExtrimeTraect(
		Vec3 pos0_pos,
		Vec3 pos0_vel,
		TFloat* vec0,
		Vec3 p_trg,
		Vec3 p_trg_extrime_dir,
		size_t &count,
		TFloat tMax,
		TFloat tFast,
		TFloat h0 = -20,
		TFloat dt = 1.0 / 23.0,
		TFloat gst_dist = 1800,
		TFloat gamma = 30,
		TFloat a = 13);

	Vec3* TraectToPoint(Vec3 p0, Vec3 v0_dir, Vec3 p_trg, TFloat temperat, size_t & count);
	bool GrammyCluster::GetExtrimeTraects(Vec3 p0, Vec3 v0_dir, Vec3 p_trg, TFloat temperat, Vec3 ***traectories, size_t & count, size_t ** counts);

private:
	TFloat *get_data_arr(size_t n);
	void set_dems_4i();

};



TFloat InterpAbstract(TFloat m50, TFloat p50, TFloat temper);
TFloat* GetInitCondition(Vec3 pos0, Vec3 v_dir0, Vec3 trg_pos, TFloat temperature, Vec3& pos_pos, Vec3& pos_vel, TFloat& time_end);
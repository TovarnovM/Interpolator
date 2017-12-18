#include"GrammyCluster.h"

GrammyCluster::GrammyCluster()
{
	dems_dx = get_dems_dx();
	dems = get_dems();
	dems_length = get_dems_length();
	n_elems = 1;
	for (size_t i = 0; i < 7; i++)
	{
		n_elems *= dems_length[i];
	}
	data_arr = get_data_arr(n_elems);
	set_dems_4i();

	par_tFast_base_part = 0.85;
	par_extrime_dL = 30000;
	par_h0 = -20;
	par_dt = 1.0 / 23.0;
	par_gst_dist = 1800;
	par_gamma = 30;
	par_a = 13;
}

GrammyCluster::~GrammyCluster()
{
	delete[] dems_dx;
	for (size_t i = 0; i < 6; i++)
	{
		delete[] dems[i];
	}
	delete[] dems;
	delete[] dems_length;
	delete[] data_arr;
}


TFloat * GrammyCluster::VecInterp(TFloat * v1, TFloat * v2, const TFloat t12, const size_t length) const
{
	TFloat* res = new TFloat[length];
	for (size_t i = 0; i < length; i++)
	{
		res[i] = v1[i] * (1 - t12) + v2[i] * t12;
	}
	return res;
}

void GrammyCluster::GrammyStep_ray(Vec3 & dist_ray, Vec3 & currPos_Pos, Vec3 & currPos_Vel, TFloat * currVec, TFloat & grammyL) const
{


	Vec3 v0 = currPos_Vel;
	Vec3 x0 = Vec3(v0.x, 0, v0.z).Norm();
	Vec3 y0(0, 1, 0);
	Vec3 z0 = cross(x0, y0);

	Vec3 ray_local(dot(dist_ray, x0), dot(dist_ray, y0), dot(dist_ray, z0));

	TFloat *grVec = GrammyVecInterp(currVec);
	Grammy gr_curr(grVec);
	delete[] grVec;
	TFloat* nextVec = gr_curr.PolygonsIntercept(Vec3(0, 0, 0), ray_local);
	Vec3 posFromGrammy_pos, posFromGrammy_vel;
	PosFromVec(nextVec, posFromGrammy_pos, posFromGrammy_vel);
	Vec3 nextPos_pos, nextPos_vel;
	GoToNextPos(currPos_Pos, currPos_Vel, posFromGrammy_pos, posFromGrammy_vel, nextPos_pos, nextPos_vel);

	currPos_Pos = nextPos_pos;
	currPos_Vel = nextPos_vel;

	for (size_t i = 0; i < 6; i++)
	{
		currVec[i] = nextVec[i];
	}

	Vec3 pos, vec;
	PosFromVec(nextVec, pos, vec);
	grammyL = pos.GetLength();

	delete[] nextVec;
}

bool GrammyCluster::HitFromThatPos(Vec3 pos0_pos, Vec3 pos0_vel, TFloat * vec0, Vec3 & p_dist, Vec3 & surf_n, Vec3 & surf_p, TFloat t_max, Vec3 * lst_pos, Vec3 * lst_vel, TFloat** lst_vec, size_t &count, TFloat dt, TFloat h0, TFloat gst_dist, TFloat gamma, TFloat a)


{
	bool hit = false;
	TFloat coneL = 0;
	TFloat t = vec0[1];
	size_t ind = 0;
	count = 0;
	while (t < t_max && GoodVec(vec0)) {
		Vec3 curr_mt_pos_pos = pos0_pos;
		Vec3 curr_mt_pos_vel = pos0_vel;
		TFloat dist = (curr_mt_pos_pos - p_dist).GetLength();
		Vec3 currTrgP;
		if (dist > gst_dist)
			currTrgP = GetSurfTarget_toPoint(surf_n, surf_p, pos0_vel, pos0_pos, p_dist, gamma, dt, a);
		else
			currTrgP = p_dist;

		GrammyStep_toPoint(currTrgP, pos0_pos, pos0_vel, vec0, coneL);
		t += dt;
		lst_pos[ind] = curr_mt_pos_pos;
		lst_vel[ind] = curr_mt_pos_vel;
		TFloat* vec_curr = new TFloat[6];
		for (size_t i = 0; i < 6; i++)
		{
			vec_curr[i] = vec0[i];
		}
		lst_vec[ind] = vec_curr;
		ind++;
		count = ind;
		if (dist < coneL * 3) {
			hit = true;
			break;
		}
		if (curr_mt_pos_pos.y < h0) {
			break;
		}
	}
	return hit;
}

Vec3 * GrammyCluster::GetExtrimeTraect(Vec3 pos0_pos, Vec3 pos0_vel, TFloat * vec0, Vec3 p_trg, Vec3 p_trg_extrime_dir, size_t & count, TFloat tMax, TFloat tFast, TFloat h0, TFloat dt, TFloat gst_dist, TFloat gamma, TFloat a)
{
	TFloat tFast_base = tFast * par_tFast_base_part;
	size_t nfast = (size_t)(tFast_base * 23) + 2;
	size_t nmax = (size_t)(tMax * 23) + 2;
	Vec3* base_traect_pos = new Vec3[nfast];
	Vec3* base_traect_vel = new Vec3[nfast];
	TFloat **base_vecs = new TFloat*[nfast];
	size_t base_count;

	Vec3* extrime_traect = new Vec3[1];
	size_t extrime_count = 0;


	Vec3 p_extrime_trg = p_trg + p_trg_extrime_dir.Norm() * par_extrime_dL;

	HitFromThatPos(pos0_pos, pos0_vel, vec0, p_extrime_trg, tFast_base, base_traect_pos, base_traect_vel, base_vecs, base_count, dt, h0, gst_dist, gamma, a);
	
	size_t i_base_max = base_count - 1;
	size_t i_base_curr = i_base_max / 2;
	size_t i_base_min = 0;
	size_t extrime_ind = 0;

	while (i_base_max - i_base_min>13) {

		Vec3 pos1_pos = base_traect_pos[i_base_curr];
		Vec3 pos1_vel = base_traect_vel[i_base_curr];
		TFloat *vec1 = new TFloat[6];
		for (size_t i = 0; i < 6; i++)
		{
			vec1[i] = base_vecs[i_base_curr][i];
		}

		Vec3* curr_traect_pos = new Vec3[nmax];
		Vec3* curr_traect_vel = new Vec3[nmax];
		TFloat **curr_vecs = new TFloat*[nmax];
		size_t curr_count;

		bool hit = HitFromThatPos(pos1_pos, pos1_vel, vec1, p_trg, tMax, curr_traect_pos, curr_traect_vel, curr_vecs, curr_count, dt, h0, gst_dist, gamma, a);
		if (hit) {
			delete[] extrime_traect;
			extrime_traect = curr_traect_pos;
			extrime_count = curr_count;
			extrime_ind = i_base_curr;
			i_base_min = i_base_curr;
			i_base_curr = (i_base_max + i_base_min) / 2;

			delete[] curr_traect_vel;
			for (size_t i = 0; i < curr_count; i++)
			{
				delete[] curr_vecs[i];
			}
			delete[] curr_vecs;
		}
		else {
			i_base_max = i_base_curr;
			i_base_curr = (i_base_max + i_base_min) / 2;

			delete[] curr_traect_pos;
			delete[] curr_traect_vel;
			for (size_t i = 0; i < curr_count; i++)
			{
				delete[] curr_vecs[i];
			}
			delete[] curr_vecs;
		}
	}
	count = extrime_ind + extrime_count;
	Vec3 *res = new Vec3[count];
	for (int i = 0; i < extrime_ind; i++) {
		res[i] = base_traect_pos[i];
	}
	for (int i = 0; i < extrime_count; i++) {
		res[i + extrime_ind] = extrime_traect[i];
	}

	delete[] base_traect_pos;
	delete[] base_traect_vel;
	for (size_t i = 0; i < base_count; i++)
	{
		delete[] base_vecs[i];
	}
	delete[] base_vecs;

	delete[] extrime_traect;

	return res;
}

Vec3 * GrammyCluster::TraectToPoint(Vec3 p0, Vec3 v0_dir, Vec3 p_trg, TFloat temperat, size_t & count)
{
	Vec3 pos_pos, pos_vel;
	TFloat tend;
	TFloat *vec0 = GetInitCondition(p0, v0_dir, p_trg, temperat, pos_pos, pos_vel, tend);
	size_t n = (size_t)(tend * 23) + 2;
	Vec3* res = new Vec3[n];
	Vec3* vels = new Vec3[n];
	TFloat **vecs = new TFloat*[n];
	
	bool isHit = HitFromThatPos(pos_pos, pos_vel, vec0, p_trg, tend, res, vels, vecs, count, par_dt, par_h0, par_gst_dist, par_gamma, par_a);
	
	for (size_t i = 0; i < count; i++)
	{
		delete[] vecs[i];
	}
	delete[] vecs;
	delete[] vels;
	return res;
}

bool GrammyCluster::GetExtrimeTraects(Vec3 p0, Vec3 v0_dir, Vec3 p_trg, TFloat temperat, Vec3 ***traectories, size_t & count, size_t ** counts)
{
	Vec3 pos_pos, pos_vel;
	TFloat tend;
	TFloat *vec0 = GetInitCondition(p0, v0_dir, p_trg, temperat, pos_pos, pos_vel, tend);
	size_t n = (size_t)(tend * 23) + 2;
	Vec3* fastest = new Vec3[n];
	Vec3* vels = new Vec3[n];
	TFloat **vecs = new TFloat*[n];
	size_t n_fastest;

	TFloat *vec1 = new TFloat[6];
	for (size_t i = 0; i < 6; i++)
	{
		vec1[i] = vec0[i];
	}
	bool isHit = HitFromThatPos(pos_pos, pos_vel, vec1, p_trg, tend, fastest, vels, vecs, n_fastest, par_dt, par_h0, par_gst_dist, par_gamma, par_a);
	

	if (isHit) {	
		*counts = new size_t[5];
		*traectories = new Vec3*[5];
		(*traectories)[0] = fastest;
		(*counts)[0] = n_fastest;
		count = 5;
		
		delete[] vec1;
		vec1 = new TFloat[6];
		for (size_t i = 0; i < 6; i++)
		{
			vec1[i] = vec0[i];
		}
		size_t n_up;
		Vec3* extrime_up = GetExtrimeTraect(fastest[0], vels[0], vec1, p_trg, Vec3(0, 1, 0),n_up, tend, tend, par_h0,par_dt, par_gst_dist, par_gamma, par_a);
		(*traectories)[1] = extrime_up;
		(*counts)[1] = n_up;

		delete[] vec1;
		vec1 = new TFloat[6];
		for (size_t i = 0; i < 6; i++)
		{
			vec1[i] = vec0[i];
		}
		size_t n_down;
		Vec3* extrime_down = GetExtrimeTraect(fastest[0], vels[0], vec1, p_trg, Vec3(0, -1, 0), n_down, tend, tend, par_h0, par_dt, par_gst_dist, par_gamma, par_a);
		(*traectories)[2] = extrime_down;
		(*counts)[2] = n_down;

		Vec3 imp_x = p_trg - p0;
		imp_x.y = 0;
		imp_x = imp_x.Norm();
		Vec3 imp_z = cross(imp_x, Vec3(0, 1, 0));

		delete[] vec1;
		vec1 = new TFloat[6];
		for (size_t i = 0; i < 6; i++)
		{
			vec1[i] = vec0[i];
		}		
		size_t n_left;
		Vec3* extrime_left = GetExtrimeTraect(fastest[0], vels[0], vec1, p_trg, Vec3(-imp_z.x, 0, -imp_z.z), n_left, tend, tend, par_h0, par_dt, par_gst_dist, par_gamma, par_a);
		(*traectories)[3] = extrime_left;
		(*counts)[3] = n_left;
		
		delete[] vec1;
		vec1 = new TFloat[6];
		for (size_t i = 0; i < 6; i++)
		{
			vec1[i] = vec0[i];
		}
		size_t n_right;
		Vec3* extrime_right = GetExtrimeTraect(fastest[0], vels[0], vec1, p_trg, imp_z, n_right, tend, tend, par_h0, par_dt, par_gst_dist, par_gamma, par_a);
		(*traectories)[4] = extrime_right;
		(*counts)[4] = n_right;
	}

	
	for (size_t i = 0; i < n_fastest; i++)
	{
		delete[] vecs[i];
	}
	delete[] vec1;
	delete[] vecs;
	delete[] vels;

	if (!isHit) {
		count = 0;
		delete[] fastest;
	}
		
	return isHit;
}

TFloat * GrammyCluster::get_data_arr(size_t n)
{
	TFloat *res = new TFloat[n];
	return res;
}

void GrammyCluster::set_dems_4i()
{
	dems_4i = new size_t[6];
	dems_4i[5] = dems_length[6];
	for (int i = 5 - 1; i >= 0; i--)
	{
		dems_4i[i] = dems_4i[i + 1] * dems_length[i + 1];
	}
}

int GrammyCluster::load_from_csv(const char* file_name)
{
	FILE *pFile;
	pFile = fopen(file_name, "r");
	if (!pFile) {
		return 13;
	}
	for (size_t i = 0; i < n_elems; i++)
	{
		fscanf(pFile, SCANF_STR, &data_arr[i]);
	}
	fclose(pFile);
	return 0;

	
}

int GrammyCluster::load_from_bin(const char * file_name)
{
	FILE *pFile;
	pFile = fopen(file_name, "rb");
	if (!pFile) {
		return 13;
	}
	fread(data_arr, sizeof(data_arr), n_elems, pFile);
	fclose(pFile);
	return 0;
}

int GrammyCluster::save_to_bin(const char * file_name)
{
	FILE *pFile;
	pFile = fopen(file_name, "wb");
	if (!pFile) {
		return 13;
	}
	fwrite(data_arr, sizeof(data_arr), n_elems, pFile);
	fclose(pFile);
	return 0;
}

TFloat * GrammyCluster::get_vec(const size_t i0, const size_t i1, const size_t i2, const size_t i3, const size_t i4, const size_t i5) const
{
	size_t i = i0*dems_4i[0]
		+ i1*dems_4i[1]
		+ i2*dems_4i[2]
		+ i3*dems_4i[3]
		+ i4*dems_4i[4]
		+ i5*dems_4i[5];
	return &data_arr[i];

}

void GrammyCluster::GetInterp(const size_t inds_n, const TFloat val, size_t & out_ind0, size_t & out_ind1, TFloat & out_t) const
{
	TFloat ind0d = (val - dems[inds_n][0]) / dems_dx[inds_n];
	size_t ind0 = (size_t)(ind0d);
	if (ind0d < 0) {
		out_ind0 = 0;
		out_ind1 = 1;
		out_t = 0;
		return;
	}
	size_t ind1 = ind0 + 1;
	if (ind1 >= dems_length[inds_n]) {
		out_ind0 = dems_length[inds_n] - 2;
		out_ind1 = dems_length[inds_n] - 1;
		out_t = 1;
		return;
	}
	else {
		out_ind0 = ind0;
		out_ind1 = ind1;
		out_t = (val - dems[inds_n][0]) / dems_dx[inds_n] - ind0;
		return;
	}
}

TFloat * GrammyCluster::GrammyVecInterp(TFloat * vBegin) const
{
	const size_t vBegin_Length = 6;
	size_t lst_ind0[vBegin_Length],
		lst_ind1[vBegin_Length];
	TFloat lst_t[vBegin_Length];
	for (size_t i = 0; i < vBegin_Length; i++)
	{
		GetInterp(i, vBegin[i], lst_ind0[i], lst_ind1[i], lst_t[i]);
	}
	const size_t n = 64;
	const size_t n_devs[vBegin_Length]{ 32,16,8,4,2,1 };
	TFloat **vecs = new TFloat*[n];
	for (size_t i = 0; i < n; i++) {
		size_t n_inds[vBegin_Length];
		for (size_t j = 0; j < vBegin_Length; j++) {
			n_inds[j] = (i % (2 * n_devs[j])) / n_devs[j] == 0
				? lst_ind0[j]
				: lst_ind1[j];

		}
		TFloat* vec = get_vec(
			n_inds[0],
			n_inds[1],
			n_inds[2],
			n_inds[3],
			n_inds[4],
			n_inds[5]);
		vecs[i] = vec;
	}

	for (size_t i = 0; i < vBegin_Length; i++) {
		TFloat ** vecs_reduce = new TFloat*[n_devs[i]];
		for (size_t j = 0; j < n_devs[i]; j++) {
			TFloat * vec_red = VecInterp(vecs[j], vecs[j + n_devs[i]], lst_t[i]);
			vecs_reduce[j] = vec_red;
		}

		if (i > 0) {
			for (size_t k = 0; k < n_devs[i - 1]; k++)
			{
				delete[] vecs[k];
			}
		}
		delete[] vecs;
		vecs = vecs_reduce;
	}

	TFloat* res = vecs[0];
	delete[] vecs;
	return res;
}

TFloat * GrammyCluster::get_vec(const size_t* inds) {
	return get_vec(inds[0], inds[1], inds[2], inds[3], inds[4], inds[5]);
}

TFloat InterpAbstract(TFloat m50, TFloat p50, TFloat temper)
{
	TFloat t = (temper + 50.0) / 100.0;
	return m50 + t * (p50 - m50);
}

TFloat * GetInitCondition(Vec3 pos0, Vec3 v_dir0, Vec3 trg_pos, TFloat temperature, Vec3 & pos_pos, Vec3 & pos_vel, TFloat & time_end)
{

	TFloat l0 = InterpAbstract(197, 151, temperature);
	TFloat time0 = InterpAbstract(2.15, 1.22, temperature);
	TFloat v0 = InterpAbstract(175, 237, temperature);
	time_end = InterpAbstract(77, 47, temperature);
	TFloat alpha0 = 3;

	Vec3 p00 = (trg_pos - pos0);


	Vec3 p0 = pos0 + v_dir0.Norm() * l0; //new Vector3D(p00.X, 0, p00.Z).Norm * l0;//(trg_pos - pos0).Norm * l0;

	Vec3 vel0 = v_dir0.Norm() * v0;// new Vector3D(p00.X, 0, p00.Z).Norm * v0;;//(trg_pos - pos0).Norm * v0;

	TFloat thetta0 = 90.0 - acos(dot(vel0.Norm(), Vec3(0, 1, 0))) * GRAD;

	TFloat *vec0 = new TFloat[6]{ temperature,
		time0,
		v0,
		alpha0,
		0,
		thetta0
	};

	pos_pos = p0;
	pos_vel = vel0;

	return vec0;

}

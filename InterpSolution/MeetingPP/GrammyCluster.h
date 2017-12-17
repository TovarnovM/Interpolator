#pragma once

#include"stdafx.h"
#include"Funcs.h"

class GrammyCluster {
public:
	TFloat *dems_dx;
	TFloat **dems;
	TFloat ******* data;
	size_t* dems_length;
	GrammyCluster();
	~GrammyCluster();
	void load_from_csv(std::string &path);

private:
	TFloat ******* get_data(size_t* n);
};

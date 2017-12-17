#include"GrammyCluster.h"

GrammyCluster::GrammyCluster()
{
	dems_dx = get_dems_dx();
	dems = get_dems();
	dems_length = get_dems_length();
	data = get_data(dems_length);
	
}

GrammyCluster::~GrammyCluster()
{
	delete[] dems_dx;
	for (size_t i = 0; i < 6; i++)
	{
		delete[] dems[i];
	}
	delete[] dems;
	for (size_t i0 = 0; i0 < dems_length[0]; i0++)
	{
		for (size_t i1 = 0; i1 < dems_length[1]; i1++)
		{
			for (size_t i2 = 0; i2 < dems_length[2]; i2++)
			{
				for (size_t i3 = 0; i3 < dems_length[3]; i3++)
				{
					for (size_t i4 = 0; i4 < dems_length[4]; i4++)
					{
						for (size_t i5 = 0; i5 < dems_length[5]; i5++)
						{
							delete[] data[i0][i1][i2][i3][i4][i5];
						}
						delete[] data[i0][i1][i2][i3][i4];
					}
					delete[] data[i0][i1][i2][i3];
				}
				delete[] data[i0][i1][i2];
			}
			delete[] data[i0][i1];
		}
		delete[] data[i0];
	}
	delete[] data;
	delete[] dems_length;
}

TFloat ******* GrammyCluster::get_data(size_t* n)
{
	TFloat******* res = new TFloat******[n[0]];
	for (size_t i0 = 0; i0 < n[0]; i0++)
	{
		res[i0] = new TFloat*****[n[1]];
		for (size_t i1 = 0; i1 < n[1]; i1++)
		{
			res[i0][i1] = new TFloat****[n[2]];
			for (size_t i2 = 0; i2 < n[2]; i2++)
			{
				res[i0][i1][i2] = new TFloat***[n[3]];
				for (size_t i3 = 0; i3 < n[3]; i3++)
				{
					res[i0][i1][i2][i3] = new TFloat**[n[4]];
					for (size_t i4 = 0; i4 < n[4]; i4++)
					{
						res[i0][i1][i2][i3][i4] = new TFloat*[n[5]];
						for (size_t i5 = 0; i5 < n[5]; i5++)
						{
							res[i0][i1][i2][i3][i4][i5] = new TFloat[n[6]];
						}
					}
				}
			}
		}
	}

	return res;
}

void GrammyCluster::load_from_csv(std::string & path)
{
	std::ifstream file(path);

	
}


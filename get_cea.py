from rocketcea.cea_obj import CEA_Obj
C = CEA_Obj( oxName='N2O', fuelName='C2H5OH')

mr_start = 3
mr_end = 6
mr_step = 0.1
pc_start = 100.0
pc_end = 1000.0
pc_step = 50.0

with open("N2O_C2H5OH.csv", "w") as f:
    f.write("O/F, Pc (psia), Isp (s), Tc (R), Tt (R), Te (R), MW (lb/lbmol), gamma\n")

for mr in range(mr_start*10, mr_end*10+1, int(mr_step*10)):
    for pc in range(int(pc_start), int(pc_end)+1, int(pc_step)):
        of = mr / 10.0
        isp = C.get_Isp(Pc=pc, MR=of)
        temperatures = C.get_Temperatures(Pc=pc, MR=of)
        mole_weight, gamma = C.get_Throat_MolWt_gamma(Pc=pc, MR=of)
        with open("N2O_C2H5OH.csv", "a") as f:
            f.write(f"{of:.2f}, {pc}, {isp:.2f}, {temperatures[0]:.2f}, {temperatures[1]:.2f}, {temperatures[1]:.2f}, {mole_weight:.4f}, {gamma:.4f}\n")

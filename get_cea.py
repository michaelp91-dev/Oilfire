from rocketcea.cea_obj import CEA_Obj
import os
import re

# --- Constants for Unit Conversion ---
FT_TO_M = 0.3048      # Feet to Meters
RANKINE_TO_KELVIN = 5.0 / 9.0  # Rankine to Kelvin
PSI_TO_ATM = 14.6959 # PSI to ATM
BAR_TO_PSI = 14.504

# --- Configuration ---
OUTPUT_FILENAME = "N2O_C3H8O.csv" # Updated output filename
DATA_SUBFOLDER = "Data"
DEFAULT_EPS = 40 # Default expansion ratio
# ----------------------------------------------------

# Initialize the CEA object for Ethanol and N2O
C = CEA_Obj(oxName="N2O", fuelName="Isopropanol")

# Create the data subfolder if it doesn't exist
if not os.path.exists(DATA_SUBFOLDER):
    os.makedirs(DATA_SUBFOLDER)

# Full path for the output file
output_path = os.path.join(DATA_SUBFOLDER, OUTPUT_FILENAME)

mr_start = 0.5
mr_end = 2.5
mr_step = 0.1
pc_start = 20.0
pc_end = 1.0
pc_step = 70.0
eps_opt_start = 1.0 # Define start for optimal eps search
eps_opt_end = 10.0 # Define end for optimal eps search
eps_opt_step = 0.1 # Define step for optimal eps search

ambient_pressure_atm = 1.0 # Assuming sea level for optimal eps

with open(output_path, "w") as f:
    # Write the CSV header
    f.write("O/F,Pc_bar,Pc_pdia,Isp_s,Tc_R,Tt_R,Te_R,MW_lbm_lbmol,Gamma,Optimal_Eps,Pe_to_Pamb,Pt_atm\n") # Added Pe_to_Pamb column

print(f"Generating data for various O/F, Pc, and Eps and saving to {output_path}...")

for mr in range(int(mr_start*10), int(mr_end*10)+1, int(mr_step*10)):
    for pc in range(int(pc_start), int(pc_end)+1, int(pc_step)):
        of = mr / 10.0
	pc_psi = pc * BAR_TO_PSI

        # Calculate performance parameters for default eps
        isp = C.get_Isp(Pc=pc_psi, MR=of, eps=DEFAULT_EPS)
        temperatures = C.get_Temperatures(Pc=pc_psi, MR=of, eps=DEFAULT_EPS)
        mole_weight, gamma = C.get_Throat_MolWt_gamma(Pc=pc_psi, MR=of, eps=DEFAULT_EPS)

        # Find optimal eps by parsing full CEA output
        optimal_eps = None
        min_pressure_diff = float('inf')
        optimal_pe_atm = None # To store the exit pressure at the optimal eps

        for eps_opt in [eps_opt_start + i * eps_opt_step for i in range(int((eps_opt_end - eps_opt_start) / eps_opt_step) + 1)]:
            try:
                full_output = C.get_full_cea_output(Pc=pc_psi, MR=of, eps=eps_opt)
                # Parse the output to find the exit pressure
                # Look for the line starting with "P, ATM" and extract the value under the "EXIT" column
                pe_atm = None
                pt_atm = None
                for line in full_output.splitlines():
                    if line.strip().startswith("P, ATM"):
                        values = line.split()
                        try:
                            pe_atm = float(values[4])
                            pt_atm = float(values[3])
                            break # Found the exit pressure, no need to parse further lines
                        except ValueError:
                             pass # "EXIT" not found in headers, continue searching


                if pe_atm is not None:
                    # Calculate pressure difference from ambient
                    pressure_diff = abs(pe_atm - ambient_pressure_atm)

                    # Update optimal eps if the current pressure difference is smaller
                    if pressure_diff < min_pressure_diff:
                        min_pressure_diff = pressure_diff
                        optimal_eps = eps_opt
                        optimal_pe_atm = pe_atm # Store the exit pressure at this optimal eps

            except Exception as e:
                # Handle cases where CEA calculation or parsing might fail
                pass # Suppress the warning for now

        # Calculate Pe/Pamb for the optimal eps
        pe_to_pamb = optimal_pe_atm / ambient_pressure_atm if optimal_pe_atm is not None else 'NaN'


        # Format the values for writing, handling None
        isp_s_str = f"{isp:.2f}" if isp is not None else 'NaN'
        pc_bar_str = f"{pc:.2f}
        pc_p
        tc_R_str = f"{temperatures[0]:.2f}" if temperatures is not None and len(temperatures) > 0 else 'NaN'
        tt_R_str = f"{temperatures[1]:.2f}" if temperatures is not None and len(temperatures) > 1 else 'NaN'
        te_R_str = f"{temperatures[2]:.2f}" if temperatures is not None and len(temperatures) > 2 else 'NaN'
        mw_lbm_lbmol_str = f"{mole_weight:.4f}" if mole_weight is not None else 'NaN'
        gamma_str = f"{gamma:.4f}" if gamma is not None else 'NaN'
        optimal_eps_str = f"{optimal_eps:.2f}" if optimal_eps is not None else 'NaN'
        pe_to_pamb_str = f"{pe_to_pamb:.4f}" if isinstance(pe_to_pamb, float) else str(pe_to_pamb)


        # Write the data row to the file
        with open(output_path, "a") as f:
            f.write(f"{of:.2f},{pc},{pc_psi},{isp_s_str},{tc_R_str},{tt_R_str},{te_R_str},{mw_lbm_lbmol_str},{gamma_str},{optimal_eps_str},{pe_to_pamb_str},{pt_atm}\n")

print("Data generation complete.")

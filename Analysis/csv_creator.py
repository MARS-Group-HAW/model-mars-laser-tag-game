import os

import pandas as pd
import glob


def load_csv():

    path = '../lasertag/src-gen/'
    all_filenames = [path+'Yellow.csv', path+'Green.csv', path+'Blue.csv', path+'Red.csv']

    map_csv = pd.read_csv(path+"Battleground.csv", delimiter=';', header=None)
    # export to csv
    map_csv.to_csv("map.csv", sep=';', index=False, encoding='utf-8-sig', header=False)

    combined_csv = pd
    for f in all_filenames:
        try:
            # combine all files in the list
            combined_csv = pd.read_csv(f.strip(), delimiter=';')

        except pd.errors.EmptyDataError:
            print('Note: {} was empty. Skipping.'.format(f.replace(path, '')))
            continue  # will skip the rest of the block and move to next file

    # sort the list
    combined_csv.sort_values(by=['Tick'], inplace=True)
    # export to csv
    combined_csv.to_csv("agents.csv", sep=';', index=False, encoding='utf-8-sig')
    # print(combined_csv)

if __name__ == '__main__':
    load_csv()

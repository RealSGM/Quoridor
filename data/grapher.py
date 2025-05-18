import json
import matplotlib.pyplot as plt
import numpy as np

data = {}
FILE_NAME = 'data/algorithm_data.json'

'''
    {
        "mcts": {
            "current_turn": 1,
            "fences_remaining_cumulative": [
            ],
            "games_played": 28.0,
            "move_speeds_cumulative": [
            ],
            "moves_made_cumulative": [
            ],
            "nodes_searched_cumulative": [
            ],
            "pawn_moves_cumulative": [
            ],
            "wins": 0.0
        },
    }
'''

def load_data():
    global data
    with open(FILE_NAME, 'r') as f:
        data = json.load(f)


def parse_data(key: str):
    if key not in data:
        return None
    print(f"Parsing data for {key}...")
    
    wins = float(data[key]['wins'])
    games_played = float(data[key]['games_played'])
    fences_cumulative = data[key]['fences_remaining_cumulative']
    moves_cumulative = data[key]['moves_made_cumulative']
    move_speeds_cumulative = data[key]['move_speeds_cumulative']
    nodes_cumulative = data[key]['nodes_searched_cumulative']
    pawn_moves_cumulative = data[key]['pawn_moves_cumulative']
    
    total_moves = sum(moves_cumulative)
    total_moves_speed = sum(move_speeds_cumulative)
    total_nodes = sum(nodes_cumulative)
    total_pawn_moves = sum(pawn_moves_cumulative)
    
    average_win_rate = wins / games_played if games_played > 0 else 0
    average_move_speed = total_moves_speed / total_moves if total_moves > 0 else 0
    average_nodes = total_nodes / games_played if games_played > 0 else 0
    average_pawn_moves = total_pawn_moves / games_played if games_played > 0 else 0
    average_game_length = total_moves / games_played if games_played > 0 else 0
    
    average_fences_remaining = []
    
    for i in range(len(fences_cumulative)):
        fences_remaining = fences_cumulative[i]
        if len(moves_cumulative) <= i:
            continue
        moves_made = moves_cumulative[i]
        average_fences = round(fences_remaining / moves_made, 2) if moves_made > 0 else 0
        average_fences_remaining.append(average_fences)
        
    print(f"Win rate for {key}: {average_win_rate:.2%}")
    print(f"Average move speed for {key}: {average_move_speed:.2f}")
    print(f"Average nodes searched for in a game of {key}: {average_nodes:.2f}")
    print(f"Average pawn moves in a game of {key}: {average_pawn_moves:.2f}")
    print(f"Average game length for {key}: {average_game_length:.2f}")
    print(f"Average fences remaining for {key}: {average_fences_remaining}")
    print("--------------------")

if __name__ == '__main__':
    load_data()
    for key in data:
        parse_data(key)
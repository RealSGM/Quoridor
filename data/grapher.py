import json
import matplotlib.pyplot as plt
import os

data = {}
FILE_NAME = 'data/algorithm_data.json'
SAVE_FILE = 'data/processed_data.json'
GRAPH_DIR = 'data/graphs'

processed_data = {}

# Ensure graph directory exists
os.makedirs(GRAPH_DIR, exist_ok=True)

def save_data():
    with open(SAVE_FILE, 'w') as f:
        json.dump(processed_data, f, indent=4)
    print(f"Processed data saved to {SAVE_FILE}")

def graph_data(data: list, name: str):
    # Plot average fences remaining
    turns = list(range(1, len(data) + 1))
    plt.figure(figsize=(10, 6))
    plt.plot(turns, data, marker='o', linestyle='-', color='blue')
    plt.title(f'Average {name.capitalize()} Over Turns')
    plt.xlabel('Turn Number')
    plt.ylabel(f'Average {name.capitalize()}')
    plt.grid(True)
    
    os.makedirs(f"{GRAPH_DIR}/{key}", exist_ok=True)

    # Save plot to file
    filename = f"{GRAPH_DIR}/{key}/{name}.png"
    plt.savefig(filename)
    plt.close()  # Close plot to free memory

    print(f"Plot saved to: {filename}")

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
    
    average_fences_remaining = [
        round(f / m, 2) if m > 0 else 0
        for f, m in zip(fences_cumulative, moves_cumulative)
    ]
    
    average_nodes = [
        round(n / m, 2) if m > 0 else 0
        for n, m in zip(nodes_cumulative, moves_cumulative)
    ]
    
    average_pawn_moves = [
        round(p / m, 2) if m > 0 else 0
        for p, m in zip(pawn_moves_cumulative, moves_cumulative)
    ]
    
    average_speeds = [
        round(s / m, 2) if m > 0 else 0
        for s, m in zip(move_speeds_cumulative, moves_cumulative)
    ]
    
    processed_data[key] = {
        'win_rate': average_win_rate,
        'move_speed': average_move_speed,
        'nodes': average_nodes,
        'pawn_moves': average_pawn_moves,
        'game_length': average_game_length
    }
    
    graph_data(average_fences_remaining, 'Fences Remaining')
    graph_data(average_nodes, 'Nodes')
    graph_data(average_pawn_moves, 'Pawn Moves')
    graph_data(average_speeds, 'Move Speeds')

if __name__ == '__main__':
    load_data()
    for key in data:
        parse_data(key)
    save_data()

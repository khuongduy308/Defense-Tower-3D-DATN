import { _decorator, Component, Node, Prefab, instantiate, Vec3 } from 'cc';
const { ccclass, property } = _decorator;

enum TileType {
    None = 0,
    Grass = 1,
    Stone = 2,
    Water = 3,
    Path = 4,
}

class TileInfo {
    x: number;
    z: number;
    type: TileType;
    node: Node;

    constructor(x: number, z: number, type: TileType, node: Node) {
        this.x = x;
        this.z = z;
        this.type = type;
        this.node = node;
    }
}


@ccclass('TDMapGenerator')
export class TDMapGenerator extends Component {
    @property({ type: Prefab })
    grassPrefab: Prefab = null;

    @property({ type: Prefab })
    stonePrefab: Prefab = null;

    @property({ type: Prefab })
    waterPrefab: Prefab = null;

    @property({ type: Prefab })
    roadPrefab: Prefab = null;

    @property
    mapWidth: number = 15;

    @property
    mapHeight: number = 15;

    @property
    blockSize: number = 1;

    private tileMap: TileInfo[][] = []; // 👈 Bắt buộc phải có dòng này!

    private currentMapNode: Node = null;

    private mapData: number[][] = [];

    generateBaseMap(): void {
        this.mapData = [];
        for (let z = 0; z < this.mapHeight; z++) {
            const row: number[] = [];
            for (let x = 0; x < this.mapWidth; x++) {
                const rand = Math.random();
                let tileType: TileType;

                if (rand < 0.5) tileType = TileType.Grass;
                else if (rand < 0.8) tileType = TileType.Stone;
                else tileType = TileType.Water;

                row.push(tileType);
            }
            this.mapData.push(row);
        }
    }

    generateRandomPath(): void {
        // Random đường đi từ trái sang phải (giả lập DFS nhẹ)
        let x = 0;
        let z = Math.floor(Math.random() * this.mapHeight);

        const path: [number, number][] = [];
        while (x < this.mapWidth && z >= 0 && z < this.mapHeight) {
            this.mapData[z][x] = TileType.Path;
            path.push([x, z]);

            const dirs: [number, number][] = [
                [1, 0], // đi tới
                [0, 1], // xuống
                [0, -1] // lên
            ];

            const validMoves = dirs.filter(([dx, dz]) => {
                const newX = x + dx;
                const newZ = z + dz;
                return (
                    newX >= 0 && newX < this.mapWidth &&
                    newZ >= 0 && newZ < this.mapHeight &&
                    this.mapData[newZ][newX] !== TileType.Path // không đi lại
                );
            });

            if (validMoves.length === 0) break;

            const [dx, dz] = validMoves[Math.floor(Math.random() * validMoves.length)];
            x += dx;
            z += dz;
        }
    }

    spawnTile(type: TileType, x: number, z: number, parent: Node): void {
        let prefab: Prefab = null;
        switch (type) {
            case TileType.Grass:
                prefab = this.grassPrefab;
                break;
            case TileType.Stone:
                prefab = this.stonePrefab;
                break;
            case TileType.Water:
                prefab = this.waterPrefab;
                break;
            case TileType.Path:
                prefab = this.roadPrefab;
                break;
        }

        if (prefab) {
            const tile = instantiate(prefab);
            tile.setParent(parent); // 👈 Gắn vào mapContainer
            tile.setPosition(new Vec3(x * this.blockSize, 0, -z * this.blockSize));

            const tileInfo = new TileInfo(x, z, type, tile);
            if (!this.tileMap[z]) this.tileMap[z] = [];
            this.tileMap[z][x] = tileInfo;
        }
    }

    generateMap() {
        this.generateBaseMap();
        this.generateRandomPath();

        // 🆕 Tạo 1 Node mới chứa toàn bộ tile
        const mapContainer = new Node(`Map_${Date.now()}`); // Tên theo timestamp
        mapContainer.setParent(this.node); // Là con của "Map"
        
        // Gán cho dùng ở các nơi khác nếu muốn
        this.currentMapNode = mapContainer;

        // 🧱 Sinh các tile gắn vào mapContainer
        for (let z = 0; z < this.mapHeight; z++) {
            for (let x = 0; x < this.mapWidth; x++) {
                const tileType = this.mapData[z][x];
                this.spawnTile(tileType, x, z, mapContainer);
            }
        }
    }

    exportMapToJson(): string {
        const data = [];

        for (let z = 0; z < this.tileMap.length; z++) {
            const row = [];
            for (let x = 0; x < this.tileMap[z].length; x++) {
                const tile = this.tileMap[z][x];
                row.push({
                    x: tile.x,
                    z: tile.z,
                    type: tile.type
                });
            }
            data.push(row);
        }

        const jsonStr = JSON.stringify(data, null, 2);
        console.log("📤 Map JSON exported:\n", jsonStr);

        return jsonStr;
    }


    start() {
        this.generateMap();

        const json = this.exportMapToJson();
    }
}

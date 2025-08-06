import { _decorator, Component, Node, Prefab, instantiate, Vec3, JsonAsset } from 'cc';
const { ccclass, property } = _decorator;

enum TileType {
    None = 0,
    Grass = 1,
    Stone = 2,
    Water = 3,
    Path = 4,
}

class TileInfo {
    constructor(
        public x: number,
        public z: number,
        public type: TileType,
        public node: Node
    ) {}
}


@ccclass('ImportMapFromJson')
export class ImportMapFromJson extends Component {
    @property({ type: JsonAsset })
    mapJsonAsset: JsonAsset = null;

    @property({ type: Node })
    mapContainer: Node = null;

    @property({ type: Prefab }) grassPrefab: Prefab = null;
    @property({ type: Prefab }) stonePrefab: Prefab = null;
    @property({ type: Prefab }) waterPrefab: Prefab = null;
    @property({ type: Prefab }) pathPrefab: Prefab = null;

    @property
    mapWidth: number = 15;

    @property
    mapHeight: number = 15;

    @property blockSize: number = 1;

    private tileMap: TileInfo[][] = [];

    start() {
        if (this.mapJsonAsset) {
            const mapData = this.mapJsonAsset.json; // 👈 đã là object
            this.importMapFromJson(mapData as { x: number, z: number, type: TileType }[][], this.mapContainer);
        }
    }

    importMapFromJson(mapData: { x: number, z: number, type: TileType }[][], parentNode: Node): void {
        for (let z = 0; z < mapData.length; z++) {
            for (let x = 0; x < mapData[z].length; x++) {
                const tile = mapData[z][x];
                this.spawnTile(tile.type, tile.x, tile.z, parentNode);
            }
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
                prefab = this.pathPrefab;
                break;
        }

        if (prefab) {
            const tile = instantiate(prefab);
            tile.setParent(parent);
            tile.setPosition(new Vec3(x * this.blockSize, 0, -z * this.blockSize));

            const tileInfo = new TileInfo(x, z, type, tile);
            if (!this.tileMap[z]) this.tileMap[z] = [];
            this.tileMap[z][x] = tileInfo;
        }
    }

    
}


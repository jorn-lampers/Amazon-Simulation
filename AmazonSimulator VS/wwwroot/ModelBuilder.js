CreateRobot = function () {
    var robot = new THREE.Mesh(
        new THREE.BoxGeometry(0.9, 0.3, 0.9),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_side.png"), side: THREE.DoubleSide }),    //LEFT
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_side.png"), side: THREE.DoubleSide }),    //RIGHT
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_top.png"), side: THREE.DoubleSide }),     //TOP
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_bottom.png"), side: THREE.DoubleSide }),  //BOTTOM
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_front.png"), side: THREE.DoubleSide }),   //FRONT
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_front.png"), side: THREE.DoubleSide }),   //BACK
        ])
    );

    robot.position.y = 0.16;

    var group = new THREE.Group();
    group.add(robot);
    group.name = "Robot";

    return group;
}


CreateShelf = function () {
    var shelf = new THREE.Mesh(
        new THREE.BoxGeometry(1.0, 2.0, 1.0),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_side.png"), side: THREE.DoubleSide }),    //LEFT
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_side.png"), side: THREE.DoubleSide }),    //RIGHT
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_top.png"), side: THREE.DoubleSide }),     //TOP
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_bottom.png"), side: THREE.DoubleSide }),  //BOTTOM
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_front.png"), side: THREE.DoubleSide }),   //FRONT
            new THREE.MeshBasicMaterial({ map: new THREE.TextureLoader().load("textures/robot_front.png"), side: THREE.DoubleSide }),   //BACK
        ])
    );

    shelf.position.y = 1.0;

    var group = new THREE.Group();
    group.add(shelf);
    group.name = "Shelf";

    return group;
}

CreateGraph = function (graph) {
    var group = new THREE.Group();
    graph.Edges.forEach(function (edge) {
        var lineGeometry = new THREE.Geometry();
        lineGeometry.vertices.push(new THREE.Vector3(edge.a.X, edge.a.Y, edge.a.Z));
        lineGeometry.vertices.push(new THREE.Vector3(edge.b.X, edge.b.Y, edge.b.Z));
        var mesh = new THREE.Line(
            lineGeometry,
            new THREE.LineBasicMaterial({ color: 0x0000ff })
        );
        group.add(mesh);
    });

    graph.Nodes.forEach(function (node) {
        var dotGeometry = new THREE.Geometry();
        dotGeometry.vertices.push(new THREE.Vector3(node.X, node.Y, node.Z));
        var mesh = new THREE.Points(
            dotGeometry,
            new THREE.PointsMaterial({ size: 8, sizeAttenuation: false, color: 0xff0000 })
        );
        mesh.name = "Vertex";
        group.add(mesh);
    });
    group.name = "Graph";

    return group;
}

CreateStorage = function (length, width) {
    var bordersize = 0.15;
    var group = new THREE.Group();

    var a = new THREE.Mesh(
        new THREE.PlaneGeometry(width, bordersize),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );
    a.position.z = 0.5 * length - 0.5 * bordersize;
    a.rotation.x = 0.5 * Math.PI;
    group.add(a);

    var b = new THREE.Mesh(
        new THREE.PlaneGeometry(width, bordersize),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );
    b.position.z = -(0.5 * length - 0.5 * bordersize);
    b.rotation.x = 0.5 * Math.PI;
    group.add(b);

    var c = new THREE.Mesh(
        new THREE.PlaneGeometry(bordersize, length),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );
    c.position.x = 0.5 * width - 0.5 * bordersize;
    c.rotation.x = 0.5 * Math.PI;
    group.add(c);

    var d = new THREE.Mesh(
        new THREE.PlaneGeometry(bordersize, length),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );
    d.position.x = -(0.5 * width - 0.5 * bordersize);
    d.rotation.x = 0.5 * Math.PI;
    group.add(d);
    group.name = "Storage";

    return group;
}


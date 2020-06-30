import * as THREE from '../lib/three.module.js';

export function wrapModel ( model, name = "NO_NAME")
{

    model.name = name;

    model.updatePosition = function ( x, y, z )
    {
        model.position.set( x, y, z );
    }

    model.updateRotation = function ( rx, ry, rz )
    {
        model.rotation.x = rx;
        model.rotation.y = ry;
        model.rotation.z = rz;
    }

    model.updateDebugUI = function ( params, scene )
    {

    }

    if(name === "robot")
    {

        model.updateDebugUI = function ( params, scene )
        {

            if(model.footprint)
            {
                scene.remove( model.footprint );
                delete model.footprint;
            }

            if(model.target)
            {
                scene.remove( model.target );
                delete model.target;
            }

            model.footprint = createSegmentWrapper( model, params );
            scene.add( model.footprint );

            model.target = createTargetWrapper( model, params.Target );
            scene.add( model.target );
        }

    }

}

// Debug function that generates a model of a robots 'monitored' area it checks to prevent collisions
export function createSegmentWrapper( robot, params )
{
    let group = new THREE.Group();

    let segments = params.Trail;

    segments.forEach(function (segment)
    {
        let lineGeometry = new THREE.Geometry();

        lineGeometry.vertices.push(new THREE.Vector3(segment.P.X, 0.1, segment.P.Y));
        lineGeometry.vertices.push(new THREE.Vector3(segment.Q.X, 0.1, segment.Q.Y));

        let col = params.Brake ? 0xff0000 : 0x00ff00;
        let mat = new THREE.LineBasicMaterial({ color: col });

        let mesh = new THREE.Line(
            lineGeometry,
            mat
        );

        group.add(mesh);
    });

    group.name = "footprint";

    return group;
}

export function createTargetWrapper( model, targetPos )
{
    let group = new THREE.Group();

    let delta = new THREE.Vector3(targetPos.X - model.position.x, 0, targetPos.Z - model.position.z);

    let dist = delta.length();
    let dir = delta.normalize();

    group.add( new THREE.ArrowHelper( dir, model.position, dist, 0xff0000, dist * 0.1, 0.2 ) );
    group.name = "target";

    return group;
}

// Debug function that generates a model of a graph (used for pathfinding)
export function createGraphWrapper(graph)
{
    let group = new THREE.Group();

    graph.Edges.forEach(function (edge)
    {
        let lineGeometry = new THREE.Geometry();
        lineGeometry.vertices.push(new THREE.Vector3(edge.A.Position.X, edge.A.Position.Y, edge.A.Position.Z));
        lineGeometry.vertices.push(new THREE.Vector3(edge.B.Position.X, edge.B.Position.Y, edge.B.Position.Z));

        let mat = new THREE.LineBasicMaterial({ color: 0x0000ff });

        if (edge.Width > 0) mat = new THREE.LineBasicMaterial({ color: 0xff0000 });

        let mesh = new THREE.Line(
            lineGeometry,
            mat
        );

        group.add(mesh);
    });

    graph.Nodes.forEach(function (node)
    {
        let dotGeometry = new THREE.Geometry();
        dotGeometry.vertices.push(new THREE.Vector3(node.Position.X, node.Position.Y, node.Position.Z));
        let mesh = new THREE.Points(
            dotGeometry,
            new THREE.PointsMaterial({ size: 8, sizeAttenuation: false, color: 0xff0000 })
        );
        mesh.name = "Vertex";
        group.add(mesh);
    });

    group.name = "Graph";

    return group;
}

export function createStorage(length, width)
{
    let bordersize = 0.15;
    let group = new THREE.Group();

    let a = new THREE.Mesh(
        new THREE.PlaneGeometry(width, bordersize),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );

    a.position.z = 0.5 * length - 0.5 * bordersize;
    a.position.y += 0.01;
    a.rotation.x = 0.5 * Math.PI;

    group.add(a);

    let b = new THREE.Mesh(
        new THREE.PlaneGeometry(width, bordersize),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );

    b.position.z = - ( 0.5 * length - 0.5 * bordersize );
    b.position.y += 0.01;
    b.rotation.x = 0.5 * Math.PI;

    group.add(b);

    let c = new THREE.Mesh(
        new THREE.PlaneGeometry(bordersize, length),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );
    c.position.x = 0.5 * width - 0.5 * bordersize;
    c.position.y += 0.01;
    c.rotation.x = 0.5 * Math.PI;

    group.add(c);

    let d = new THREE.Mesh(
        new THREE.PlaneGeometry(bordersize, length),
        new THREE.MeshFaceMaterial([
            new THREE.MeshBasicMaterial({ color: "#ffff00", side: THREE.DoubleSide })
        ])
    );

    d.position.x = -( 0.5 * width - 0.5 * bordersize ); 0
    d.position.y += 0.01;
    d.rotation.x = 0.5 * Math.PI;

    group.add(d);
    group.name = "Storage";

    return group;
}


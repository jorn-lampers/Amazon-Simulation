import * as THREE from '../lib/three.module.js';

var TS = performance.now();

export function wrapModel ( model, name = "NO_NAME" )
{
    model.name = name;
    let box = new THREE.Box3().setFromObject( model );

    model.size = new THREE.Vector3();
    box.getSize( model.size );

    model.centerOffset = new THREE.Vector3();
    box.getCenter( model.centerOffset );
    model.centerOffset.add( model.position.clone().negate() );

    model.updatePosition = function ( x, y, z )
    {
        model.position.set( x, y, z );
        model.updateHitbox();
    }

    model.updateRotation = function ( rx, ry, rz )
    {
        model.rotation.x = rx;
        model.rotation.y = ry;
        model.rotation.z = rz;
    }

    model.getCenter = function ()
    {
        let boxCenter = new THREE.Vector3( 0, 0, 0 );

        boxCenter.add( this.centerOffset );
        boxCenter.add( this.position );

        return boxCenter;
    }

    model.updateHitbox = function ()
    {
        let center = this.getCenter();

        this.hitbox = new THREE.Box3();
        this.hitbox.setFromCenterAndSize( center, this.size );

        //this.hitbox.applyMatrix4( model.matrixWorld );

        this.hitboxDisplay = new THREE.Box3Helper( this.hitbox, 0xff0000 );
        this.hitboxDisplay.updateMatrixWorld( true );
    }

    model.updateHitbox();
}

export function findIntersects( raycaster, objects )
{
    let intersects = [];
    let distances = [];

    let ray = raycaster.ray;

    for (let i = 0; i < objects.length; i++)
    {
        let obj = Object.values( objects )[i];
        let hb = obj.hitbox;

        let intersection = ray.intersectBox( hb );

        if ( intersection != null )
        {
            let distance = intersection.distanceTo( ray.origin );
            if(isNaN(distance)) continue;

            // If cursor is projected on object's hitbox add obj to current intersects
            intersects.push( obj );

            distances.push( distance );
        }

    }

    return {
        objects: intersects,
        distances: distances
    };
}

export function nearestIndex( intersects )
{
    let nearestIndex = 0;
    for ( let i = 1; i < intersects.distances.length; i++ )
    {
        if(intersects.distances[nearestIndex] > intersects.distances[i])
        {
            nearestIndex = i;
        }
    }

    return nearestIndex;
}

export function timer()
{
    let diff = performance.now() - TS;
    TS = performance.now();
    return diff;
}

export function makeDraggable( elmnt )
{
    var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;

    if (document.getElementById(elmnt.id + "header")) {
        // if present, the header is where you move the DIV from:
        document.getElementById(elmnt.id + "header").onmousedown = dragMouseDown;
    } else {
        // otherwise, move the DIV from anywhere inside the DIV:
        elmnt.onmousedown = dragMouseDown;
    }

    function dragMouseDown(e) {
        e = e || window.event;
        e.preventDefault();
        // get the mouse cursor position at startup:
        pos3 = e.clientX;
        pos4 = e.clientY;
        document.onmouseup = closeDragElement;
        // call a function whenever the cursor moves:
        document.onmousemove = elementDrag;
    }

    function elementDrag(e) {
        e = e || window.event;
        e.preventDefault();
        // calculate the new cursor position:
        pos1 = pos3 - e.clientX;
        pos2 = pos4 - e.clientY;
        pos3 = e.clientX;
        pos4 = e.clientY;
        // set the element's new position:
        elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
        elmnt.style.left = (elmnt.offsetLeft - pos1) + "px";
    }

    function closeDragElement() {
        // stop moving when mouse button is released:
        document.onmouseup = null;
        document.onmousemove = null;
    }

}

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


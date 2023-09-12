main:
    movconst    $0, 255
    movconst    $1, 255
    movconst    $2, 2
    movconst    $3, 64
    mult        $0, $0
    mult        $1, $2
    mult        $3, $3
    add         $0, $1
    store       $3, $0
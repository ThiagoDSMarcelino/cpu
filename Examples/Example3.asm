main:
    movconst    $0, 255
    mult        $0, $0

    movconst    $1, 255
    movconst    $2, 2
    mult        $1, $2
    add         $0, $1

    movconst    $3, 64
    mult        $3, $3
    
loop:
    store       $3, $0
    inc         $3
    jump        loop
function [res1] = VPL_DTO_mat(allnodes, supportnodes, massnodes, allconn, ...
               passconn1, g1conn, g2conn, g3conn, g4conn, g5conn, workfolder, ...
               response_spectrum, fem_list, opt_list)
    % Retrieve nodes and connectivity
    Node = allnodes;
    for i = 1:length(supportnodes)
        Supp(i,1) = supportnodes(1,i);
        Supp(i,2) = 1;
        Supp(i,3) = 1;
    end
    for i = 1:size(allconn)
        [~,n] = size(allconn(i,:));
        tempElement = 0;
        for j = 1:n
            if allconn(i,j)~= 0
                tempElement(1,j) = allconn(i,j);
            else
                break;
            end
        end
        Element{i,1} = tempElement;
    end
    
    % Elements in anti-clockwise
    for i=1:size(Element)
        [~,n] = size(Element{i,1});
        tempPoint = 0;
        for j = 1:n
            tempPoint(j,1) = Node(Element{i,1}(j),1);
            tempPoint(j,2) = Node(Element{i,1}(j),2);
        end
        c = [mean(tempPoint(:,1)), mean(tempPoint(:,2))]; % centroid of the points
        d = tempPoint - c; % vectors from points to centroid
        angles = atan2d(d(:,1), d(:,2)); % angles from centroid to each point
        [~,idx] = sort(angles, 'descend'); % sort the angles anti-clockwise
        sortedPoint = tempPoint(idx, :); % sort the points based on the angles
        temp = 0;
        for j = 1:n
            indSort = find(ismember(Node,sortedPoint(j,:),'rows'));
            temp(1,j) = indSort;
        end
        Element1{i,1} = temp;
    end
    Element = Element1;
    
    % Passive elements and subregions 
    passconn = [];
    if isempty(passconn1) == 0
        passconn = passconn1;
    end    
    ElemInd = {};
    num_of_G = 1;
    NConstr_flag = 0;
    if isempty(g1conn) == 0
        ElemInd{num_of_G,1} = g1conn';
        num_of_G = num_of_G + 1;
        NConstr_flag = 1;
    end
    if isempty(g2conn) == 0
        ElemInd{num_of_G,1} = g2conn';
        num_of_G = num_of_G + 1;
    end
    if isempty(g3conn) == 0
        ElemInd{num_of_G,1} = g3conn';
        num_of_G = num_of_G + 1;
    end
    if isempty(g4conn) == 0
        ElemInd{num_of_G,1} = g4conn';
        num_of_G = num_of_G + 1;
    end
    if isempty(g5conn) == 0
        ElemInd{num_of_G,1} = g5conn';
    end
    
    % Objective function
    if fem_list(1) == 1
        Obj = 'Compliance';
    elseif fem_list(1) == 2
        Obj = 'Energy';
    elseif fem_list(1) == 3
        Obj = 'U_DOF';
    end
    alpha = 0.05; beta = (1+alpha)^2/4; gamma = (1+2*alpha)/2;%HHT-alpha param.
    O = zeros(2*size(Node,1),1);
    
    % Mass management
    NStep = length(response_spectrum) - 1;
    temp1 = zeros(2*size(Node,1),1);
    for i =1:length(massnodes)
        Load(i,:) = zeros(1,2*(NStep+1)+1); Load(i,1) = massnodes(1,i);
        N_M(i,1) = Load(i,1);
        N_M(i,2) = fem_list(3);

        temp1(2*N_M(i,1) - 1,1) = 1;
    end

    %% ---------------------------------------------------- CREATE 'fem' STRUCT
    fem = struct(...
          'NNode',size(Node,1),...           % Number of nodes
          'NElem',size(Element,1),...        % Number of elements
          'Node',Node,...                    % [NNode x 2] array of nodes
          'Element',{Element},...            % [NElement x Var] cell array of elements
          'Supp',Supp,...                    % Array of supports
          'Load',Load,...                    % Array of loads
          'Mass',N_M,...                     % Array of lumped masses
          'u0',O,...                         % Initial displacement vector
          'v0',O,...                         % Initial velocity vector  
          'Thickness',fem_list(7),...        % Element thickness
          'E0',fem_list(4),...               % Young's modulus of solid material
          'Nu0',fem_list(5),...              % Poisson's ratio of solid material  
          'rho',fem_list(6),...              % Mass density of solid material (kg/m^3)
          'Ar',[fem_list(8),fem_list(9)],... % Rayleigh damping param. C=Ar(1)*M+Ar(2)*K
          'ag',response_spectrum,...         % Ground acceleration  'ag',[5*sin(2.5*pi*t)], response_2_7(:,2)'
          'Tmax',fem_list(2),...             % Simulation time
          'NStep',NStep, ...                 % Total number of steps  
          'alpha', alpha, ...                % alpha parameter for HHT-alpha method
          'beta', beta, ...                  % beta parameter for HHT-alpha method
          'gamma',gamma, ...                 % gamma parameter for HHT-alpha method  
          'Obj',Obj,...                      % Objective function
          'LL',temp1,...                     % Vector of DOF index for U_DOF objective
          'Reg',fem_list(10) ...             % Tag for regular meshes
           );
    %% ---------------------------------------------------- CREATE 'opt' STRUCT
    % Multiple constraint from grasshoper
    [NConstr, ~] = size(ElemInd);
    if NConstr_flag == 0
        NConstr = 0;
    end
    if NConstr > 0
        fem.SElem = passconn'; % Elements in passive solid regions
        for ii=1:NConstr; ElemInd1{ii,1} = setdiff(ElemInd{ii,1},fem.SElem); end
    else
        fem.SElem = passconn'; % Elements in passive solid regions
        ElemInd{1} = (1:fem.NElem)'; % Element indices for volume constraint j
        ElemInd1{1} = setdiff(ElemInd{1},fem.SElem);
    end
    ElemInd = ElemInd1;
    ElemArea = Areas(fem);
    if NConstr > 0
        Vmax = opt_list(2)*sum(ElemArea(ElemInd{NConstr,1}));
        for ii=1:NConstr; VolFrac(ii,1) = Vmax/sum(ElemArea(ElemInd{ii,1})); end
    else
        Vmax = opt_list(2)*sum(ElemArea(ElemInd{1,1}));
        VolFrac(1,1) = Vmax/sum(ElemArea(ElemInd{1,1}));
    end
    
    % Filter
    q = 1;
    if opt_list(3) == 0
        P = PolyFilter(fem,opt_list(1),q);
    elseif opt_list(3) == 1
        P = PolyFilter(fem,opt_list(1),q,'X');
    elseif opt_list(3) == 2
        P = PolyFilter(fem,opt_list(1),q,'Y');
    end
    zIni = ones(size(P,2),1); 
    for ii=1:length(VolFrac); zIni(ElemInd{ii,1})=VolFrac(ii); end % Initial DVs    
    opt = struct(...               
      'zMin',0,...                 % Lower bound for design variables
      'zMax',1.0,...               % Upper bound for design variables
      'zIni',zIni,...              % Initial design variables
      'MatIntFnc',0,...            % Handle to material interpolation fnc.
      'P',P,...                    % Matrix that maps design to element vars.
      'VolFrac',VolFrac,...        % Arrat if specified volume fraction const.
      'NConstr',size(VolFrac,1),...% Number of volume constraints
      'ElemInd',{ElemInd},...      % Element indices assoc. with each constr.  
      'Tol',0.01,...               % Convergence tolerance on design vars.
      'MaxIter',opt_list(4),...    % Max. number of optimization iterations
      'Move',0.2,...               % Allowable move step in OC update scheme
      'Eta',0.5 ...                % Exponent used in OC update scheme
       );
    
    fem = PreComputations(fem); % Run preComputations
    % save parameters
    filename = strcat(workfolder, '\parameters.mat');
    save(filename, 'fem', 'opt');

    B = 1; p_i = 0:1.5:9;
    for ii=1:length(p_i)        %Continuation on the RAMP penalty parameter
      if ii==length(p_i); opt.MaxIter = opt_list(5); end
      opt.MatIntFnc = @(y)MatIntFnc(y,'RAMP-H1',[p_i(ii),B,0.5]);
      [opt.zIni,V,E,fem,f1,g1,Change1,Optf1,u_v_a] = PolyDyna(fem,opt);
      B = min(B+2,10);
      baseFileNameF = strcat(workfolder, '\result_', '_pi=', num2str(p_i(ii)), '.mat');
      baseFileNameI = strcat(workfolder, '\result_', '_pi=', num2str(p_i(ii)), '.jpg');
      temp1 = opt.zIni;
      temp2 = fem.Element;
      temp3 = fem.NElem;
      temp4 = fem.Node;
      if p_i(ii) == 9
        save(baseFileNameF,'temp1','V', 'temp2', 'temp3', 'temp4', 'f1', 'g1', 'Change1', 'Optf1', 'u_v_a', 'temp1', 'VolFrac');
      end
      [FigHandle] = InitialPlot(fem,V);
      saveas(FigHandle,baseFileNameI);

    end
    res1 = opt.zIni;
end

%------------------------------------------------------------- INITIAL PLOT
function [handle] = InitialPlot(fem,z0)
ElemNodes = cellfun(@length,fem.Element); %Number of nodes of each element
Faces = NaN(fem.NElem,max(ElemNodes));    %Populate Faces with NaN
for el = 1:fem.NElem; Faces(el,1:ElemNodes(el)) = fem.Element{el}(:); end
patch('Faces',Faces,'Vertices',fem.Node,'FaceVertexCData',0.*z0,...
      'FaceColor','flat','EdgeColor','k','linewidth',2);
handle = patch('Faces',Faces,'Vertices',fem.Node,'FaceVertexCData',...
                1-z0,'FaceColor','flat','EdgeColor','none');
axis equal; axis off; axis tight; colormap(gray); caxis([0 1]);
end
%-------------------------------------------------------------------------%
